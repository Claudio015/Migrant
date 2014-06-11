﻿// *******************************************************************
//
//  Copyright (c) 2011-2014, Antmicro Ltd <antmicro.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// *******************************************************************
using System;
using Antmicro.Migrant.Customization;
using System.IO;
using System.Reflection;

namespace Antmicro.Migrant.Tests
{
    public class TwoDomainsDriver : MarshalByRefObject
    {
        public TwoDomainsDriver(bool useGeneratedSerializer, bool useGeneratedDeserializer)
        {
            this.useGeneratedDeserializer = useGeneratedDeserializer;
            this.useGeneratedSerializer = useGeneratedSerializer;
        }

        public TwoDomainsDriver()
        {
        }

        public void PrepareDomains()
        {
            domain1 = AppDomain.CreateDomain("domain1", null, Environment.CurrentDirectory, string.Empty, true);
            domain2 = AppDomain.CreateDomain("domain2", null, Environment.CurrentDirectory, string.Empty, true);

            testsOnDomain1 = (TwoDomainsDriver)domain1.CreateInstanceAndUnwrap(typeof(TwoDomainsDriver).Assembly.FullName, typeof(TwoDomainsDriver).FullName);
            testsOnDomain2 = (TwoDomainsDriver)domain2.CreateInstanceAndUnwrap(typeof(TwoDomainsDriver).Assembly.FullName, typeof(TwoDomainsDriver).FullName);
        }

        public void DisposeDomains()
        {
            testsOnDomain1 = null;
            testsOnDomain2 = null;
            AppDomain.Unload(domain1);
            AppDomain.Unload(domain2);
            File.Delete(AssemblyName.Name + ".dll");
        }

        public void CreateInstanceOnAppDomain(DynamicClass type)
        {
            obj = type.Instantiate();
        }

        public byte[] SerializeOnAppDomain()
        {
            var t = obj.GetType();
            Console.WriteLine("Type is " + t);
            foreach(var f in t.GetFields())
            {
                Console.WriteLine("Field: {0} = {1}", f.Name, f.GetValue(obj));
            }


            var stream = new MemoryStream();
            var serializer = new Serializer();
            serializer.Serialize(obj, stream);
            return stream.ToArray();
        }

        public void SetValueOnAppDomain(string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName);
            field.SetValue(obj, value);
        }

        public object GetValueOnAppDomain(string fieldName)
        {
            var field = obj.GetType().GetField(fieldName);
            return field.GetValue(obj);
        }

        public void DeserializeOnAppDomain(byte[] data, Settings settings)
        {
            var stream = new MemoryStream(data);
            var deserializer = new Serializer(settings);
            obj = deserializer.Deserialize<object>(stream);

            var t = obj.GetType();
            Console.WriteLine("Type is " + t);
            foreach(var f in t.GetFields())
            {
                Console.WriteLine("Field: {0} = {1}", f.Name, f.GetValue(obj));
            }
        }

        public bool SerializeAndDeserializeOnTwoAppDomains(DynamicClass domainOneType, DynamicClass domainTwoType, VersionToleranceLevel vtl)
        {
            testsOnDomain1.CreateInstanceOnAppDomain(domainOneType);
            testsOnDomain2.CreateInstanceOnAppDomain(domainTwoType);

            var bytes = testsOnDomain1.SerializeOnAppDomain();
            try 
            {
                testsOnDomain2.DeserializeOnAppDomain(bytes, GetSettings(vtl));
                return true;
            } 
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected Settings GetSettings(VersionToleranceLevel level = 0)
        {
            return new Settings(useGeneratedSerializer ? Method.Generated : Method.Reflection,                  
                                useGeneratedDeserializer ? Method.Generated : Method.Reflection,                    
                                level);
        }

        protected TwoDomainsDriver testsOnDomain1;
        protected TwoDomainsDriver testsOnDomain2;

        private AppDomain domain1;
        private AppDomain domain2;

        private bool useGeneratedSerializer;
        private bool useGeneratedDeserializer;

        protected object obj;

        private static readonly AssemblyName AssemblyName = new AssemblyName("TestAssembly");
    }
}

