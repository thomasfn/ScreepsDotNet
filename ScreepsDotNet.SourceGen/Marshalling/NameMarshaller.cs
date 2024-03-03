using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class NameMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            var type = paramTypeSymbol.ToDisplayString();
            return type == "ScreepsDotNet.Interop.Name" || type == "ScreepsDotNet.Interop.Name?";
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {

                emitter.WriteLine($"if ({clrParamName} != null)");
                emitter.OpenScope();
                emitter.WriteLine($"{jsParamName} = new({clrParamName}.Value);");
                emitter.WriteLine($"Name.CopyIfNeeded({clrParamName}.Value.NameIndex);");
                emitter.CloseScope();
                emitter.WriteLine($"else");
                emitter.OpenScope();
                emitter.WriteLine($"{jsParamName} = InteropValue.Void;");
                emitter.CloseScope();
            }
            else
            {
                emitter.WriteLine($"{jsParamName} = new({clrParamName});");
                emitter.WriteLine($"ScreepsDotNet.Interop.Name.CopyIfNeeded({clrParamName}.NameIndex);");
            }
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return type == "ScreepsDotNet.Interop.Name" || type == "ScreepsDotNet.Interop.Name?";
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.WriteLine($"return {jsParamName}.AsNameNullable();");
            }
            else
            {
                emitter.WriteLine($"return {jsParamName}.AsName();");
            }
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Nme, paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
