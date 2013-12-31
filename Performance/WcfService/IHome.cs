using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService
{
    [ServiceContract]
    public interface IHome
    {
        [OperationContract]
        [WebGet(ResponseFormat=WebMessageFormat.Json, UriTemplate="?name={name}&x={x}&y={y}&e={e}")]
        MyClass GetData(string name, int x, int y, MyEnum e);
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Home : IHome
    {
        public MyClass GetData(string name, int x, int y, MyEnum e)
        {
            return new MyClass { Name = name, Sum = (x + y) * (int)e };
        }
    }

    [DataContract]
    public class MyClass
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Sum { get; set; }
    }

    public enum MyEnum
    {
        A = 2,
        B = 3,
        C = 4
    }
}
