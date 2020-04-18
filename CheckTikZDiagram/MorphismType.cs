using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    public enum MorphismType
    {
        OneMorphism,
        TwoMorphism,
        ThreeMorphism,
        FourMorphism,
        Functor,
        ContravariantFunctor,
        Bifunctor, // 2変数の関手
        NaturalTransformation,
        Modification,
        Perturbation,
        Unknown
    }
}
