namespace Depso.Generators.Scope;

public class ScopedCreateMethodsGenerator : CreateMethodsGenerator
{
	public override void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(x => Generate(x, Lifetime.Scoped));
	}
}