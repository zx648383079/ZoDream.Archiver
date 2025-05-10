using Mono.Cecil;
using System;

namespace ZoDream.BundleExtractor.Unity.Document.Cecil
{
    internal static class ResolutionExtensions
    {
        public static TypeDefinition CheckedResolve(this TypeReference type)
        {
            return Resolve(type, reference => reference.Resolve());
        }

        public static MethodDefinition CheckedResolve(this MethodReference method)
        {
            return Resolve(method, reference => reference.Resolve());
        }

        private static TDefinition Resolve<TReference, TDefinition>(TReference reference, Func<TReference, TDefinition> resolve)
            where TReference : MemberReference
            where TDefinition : class, IMemberDefinition
        {
            if (reference.Module == null)
                throw new ResolutionException(reference);

            var definition = resolve(reference);
            if (definition == null)
                throw new ResolutionException(reference);

            return definition;
        }
    }
}
