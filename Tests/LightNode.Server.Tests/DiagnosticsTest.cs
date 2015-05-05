using System;
using Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Owin.Testing;
using System.Diagnostics.Tracing;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class DiagnosticsTest
    {
        [TestMethod, Ignore]
        public void LightNodeEventSourceLogging()
        {
            var listener = new MockEventListener();
            listener.EnableEvents(LightNode.Diagnostics.LightNodeEventSource.Log, EventLevel.LogAlways);

            var testServer = TestServer.Create(app =>
            {
                var option = new LightNodeOptions
                {
                    Logger = LightNode.Diagnostics.LightNodeEventSource.Log
                };

                app.UseLightNode(option, typeof(MockEnv).Assembly);
            });

            (listener.EventList.Count > 0).IsTrue();
        }
    }

    class MockEventListener : EventListener
    {
        public List<EventWrittenEventArgs> EventList = new List<EventWrittenEventArgs>();

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            EventList.Add(eventData);
        }
    }

    public class ComplexContract : LightNodeContract
    {
        public Person CreatePerson(int age, string name)
        {
            return new Person { Age = age, Name = name };
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}