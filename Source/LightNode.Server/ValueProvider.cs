using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class ValueProvider
    {
        static readonly Regex nameRegex = new Regex("name=[\"]?([^\"]+)($| |\")", RegexOptions.Compiled);
        static readonly Regex boundaryRegex = new Regex("boundary=[\"]?([^\"]+)($| |\")", RegexOptions.Compiled);

        // object is List[String] or String(or byte[])
        // optimize way, value is single in many cases.
        Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ValueProvider(IDictionary<string, object> environment, AcceptVerbs verb)
        {
            var queryString = environment["owin.RequestQueryString"] as string;
            AppendValues(queryString);

            if (verb != AcceptVerbs.Get)
            {
                var requestHeader = environment[OwinConstants.RequestHeaders] as IDictionary<string, string[]>;
                string[] contentType;
                if (requestHeader.TryGetValue("Content-Type", out contentType))
                {
                    if (contentType.Any(x => x.Contains("application/x-www-form-urlencoded")))
                    {
                        var requestStream = environment["owin.RequestBody"] as Stream;
                        using (var sr = new StreamReader(new UnclosableStream(requestStream)))
                        {
                            var formUrlEncoded = sr.ReadToEnd();
                            AppendValues(formUrlEncoded);
                        }
                        requestStream.Position = 0; // rewind for custom use
                    }
                    else if (contentType.Any(x => x.Contains("multipart/form-data")))
                    {
                        var requestStream = environment["owin.RequestBody"] as Stream;
                        string boundary = boundaryRegex.Match(contentType[0]).Groups[1].Value;
                        ParseMultipartValue(requestStream, ("--" + boundary).ToCharArray());
                    }
                }
            }
        }

        void AppendValues(string urlEncodedString)
        {
            foreach (var amp in urlEncodedString.Split('&'))
            {
                var item = amp.Split('=');
                if (item.Length == 2)
                {
                    var key = System.Net.WebUtility.UrlDecode(item[0]);
                    var value = System.Net.WebUtility.UrlDecode(item[1]);

                    object result;
                    if (values.TryGetValue(key, out result))
                    {
                        if (result is string)
                        {
                            // second
                            values[key] = new List<string>() { (string)result, value };
                        }
                        else
                        {
                            // third
                            ((List<string>)result).Add(value);
                        }
                    }
                    else
                    {
                        // first
                        values[key] = value;
                    }
                }
            }
        }

        /// <summary>Returns List[String] or String or Null or byte[]</summary>
        public object GetValue(string key)
        {
            object result;
            return values.TryGetValue(key, out result)
                ? result
                : null;
        }

        enum ReadMultipartState
        {
            ReadingBoundary,
            ReadingHeader,
            ReadingValue
        }

        // TODO:Experimental Support
        void ParseMultipartValue(Stream stream, char[] boundary)
        {
            var state = ReadMultipartState.ReadingBoundary;

            var headerBuffer = new MemoryStream();
            var buffer = new MemoryStream();
            var bufferInReadingBoundary = new MemoryStream();
            var boundaryLength = 0;
            var lastBufferIsString = false;
            string lastName = null;

            int intB;
            while ((intB = stream.ReadByte()) != -1)
            {
                // checking boundary
                var b = (byte)intB;
                if (b == boundary[boundaryLength++])
                {
                    if (boundaryLength == boundary.Length)
                    {
                        // matched, go to next sequence.
                        boundaryLength = 0;
                        bufferInReadingBoundary.Position = 0;
                        state = ReadMultipartState.ReadingHeader;

                        // flush buffer
                        if (buffer.Position != 0)
                        {
                            if (lastName == null) break;

                            var key = lastName;

                            if (lastBufferIsString)
                            {
                                var bytes = buffer.GetBuffer();
                                var value = Encoding.UTF8.GetString(bytes, 0, (int)buffer.Position).Trim('\r', '\n');

                                object result;
                                if (values.TryGetValue(key, out result))
                                {
                                    if (result is string)
                                    {
                                        // second
                                        values[key] = new List<string>() { (string)result, value };
                                    }
                                    else
                                    {
                                        // third
                                        ((List<string>)result).Add(value);
                                    }
                                }
                                else
                                {
                                    // first
                                    values[key] = value;
                                }
                            }
                            else
                            {
                                var bytes = buffer.GetBuffer();
                                var preSkipCount = 0;
                                var postSkipCount = 0;
                                if (bytes.Length > 4)
                                {
                                    if (bytes[0] == '\r')
                                    {
                                        preSkipCount++;
                                        if (bytes[1] == '\n') preSkipCount++;
                                    }
                                    if (bytes[buffer.Position - 1] == '\n')
                                    {
                                        postSkipCount++;
                                        if (bytes[buffer.Position - 2] == '\r') postSkipCount++;
                                    }
                                }

                                var value = new byte[buffer.Position - preSkipCount - postSkipCount];
                                Buffer.BlockCopy(bytes, preSkipCount, value, 0, value.Length);

                                object result;
                                if (values.TryGetValue(key, out result))
                                {
                                    if (result is byte[])
                                    {
                                        // second
                                        values[key] = new List<byte[]>() { (byte[])result, value };
                                    }
                                    else
                                    {
                                        // third
                                        ((List<byte[]>)result).Add(value);
                                    }
                                }
                                else
                                {
                                    // first
                                    values[key] = value;
                                }
                            }

                            lastBufferIsString = false;
                            lastName = null;
                            buffer.Position = 0;
                        }

                        continue;
                    }
                    else
                    {
                        bufferInReadingBoundary.WriteByte(b);
                    }
                    // check new boundary
                    continue;
                }
                else
                {
                    if (bufferInReadingBoundary.Position != 0)
                    {
                        var bufArray = bufferInReadingBoundary.GetBuffer();
                        for (int i = 0; i < bufferInReadingBoundary.Position; i++)
                        {
                            buffer.WriteByte(bufArray[i]);
                        }
                        bufferInReadingBoundary.Position = 0;
                    }

                    boundaryLength = 0;
                }

                if (state == ReadMultipartState.ReadingHeader)
                {
                    // ReadLine
                    headerBuffer.WriteByte(b);

                    bool foundR = false;
                    while ((intB = stream.ReadByte()) != -1)
                    {
                        b = (byte)intB;
                        if (b == '\r')
                        {
                            foundR = true;
                            continue;
                        }
                        else if (foundR && b == '\n')
                        {
                            foundR = false;

                            // finish buffer.
                            var stringBuffer = headerBuffer.GetBuffer();
                            var headerString = Encoding.UTF8.GetString(stringBuffer, 0, (int)headerBuffer.Position).Trim('\r', '\n');

                            if (headerString.StartsWith("Content-Type") && headerString.Contains("text/plain"))
                            {
                                lastBufferIsString = true;
                            }
                            if (headerString.StartsWith("Content-Disposition"))
                            {
                                lastName = nameRegex.Match(headerString).Groups[1].Value;
                                state = ReadMultipartState.ReadingValue;
                            }

                            headerBuffer.Position = 0;
                            break;
                        }

                        headerBuffer.WriteByte(b);
                    }
                }
                else
                {
                    buffer.WriteByte(b);
                }
            }
        }
    }
}
