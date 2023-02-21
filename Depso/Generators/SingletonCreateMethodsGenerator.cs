namespace Depso.Generators;

public class SingletonCreateMethodsGenerator : CreateMethodsGenerator
{
	public override void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(x => Generate(x, Lifetime.Singleton));
	}
}