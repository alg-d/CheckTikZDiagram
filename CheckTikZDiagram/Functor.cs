using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CheckTikZDiagram
{
    public class Functor
    {
        static private readonly Regex _construct = new Regex(@"^Functor(.*)$");
        private readonly MathObject _functor;

        public MathObject Name { get; }

        public Functor(MathObject name, MathObject functor)
        {
            Name = name;
            _functor = functor;
        }

        public static Functor Create(string name, string functor)
        {
            var nameObj = new MathObjectFactory(name).CreateSingle();
            var functorObj = new MathObjectFactory(functor).CreateSingle();
            return new Functor(nameObj, functorObj);
        }

        public static Functor? Create(string text)
        {
            var m = _construct.Match(text);
            if (m.Success)
            {
                var math = new MathObjectFactory(m.Groups[1].Value).Create().ToArray();
                if (math.Length == 2)
                {
                    return new Functor(math[0], math[1]);
                }
            }
            return null;
        }

        public IEnumerable<MathObject> Evaluate(Dictionary<string, MathObject> parameters)
        {
            return _functor.ApplyParameters(parameters, false);
        }

        public override string ToString() => $"Functor: {Name} = {_functor}";
    }
}
