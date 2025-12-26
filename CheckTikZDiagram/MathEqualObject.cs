using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    public class MathEqualObject : MathObject
    {
        private static MathEqualObject? _instance;

        public static MathEqualObject Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MathEqualObject();
                }
                return _instance;
            }
        }

        private MathEqualObject()
        {
        }

        public override string OriginalText => "等号";

        public override TokenString Main => throw new NotImplementedException();

        public override int Length => throw new NotImplementedException();

        public override IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<(MathObject left, MathObject center, MathObject right)> Divide(MathObject center)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetVariables()
        {
            throw new NotImplementedException();
        }

        public override bool IsCategory()
        {
            throw new NotImplementedException();
        }

        public override bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters, Func<MathObject, MathObject, bool>? equalsFunc = null)
        {
            throw new NotImplementedException();
        }

        public override MathSequence SetBracket(Token left, Token right)
        {
            throw new NotImplementedException();
        }

        public override MathSequence SetScript(Token supOrSub, Token left, MathObject math, Token right)
        {
            throw new NotImplementedException();
        }

        public override TokenString ToTokenString()
        {
            throw new NotImplementedException();
        }
    }
}
