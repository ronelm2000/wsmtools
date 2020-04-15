using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    public static class DynamicExtensions
    {
        public static dynamic AsOptional(dynamic obj) {
            return new OptionalDynamicObject(obj);
        }
    }

    internal class OptionalDynamicObject : DynamicObject
    {
        private dynamic obj;

        public OptionalDynamicObject(dynamic obj)
        {
            this.obj = obj;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var objAsDictionary = (IDictionary<string, object>)obj;
                if (objAsDictionary != null && objAsDictionary.TryGetValue(binder.Name, out object innerResult))
                    result = innerResult;
                else
                    result = null;
                return true;
            }
            catch (Exception)
            {
                result = null;
                return true;
            }
        }
    }
}
