using System;

namespace Depso;

public class Disposable
{
	public static IDisposable Empty => EmptyDisposable.Instance;

	public static IDisposable Create(Action action)
	{
		return new ActionDisposable(action);
	}

	private class EmptyDisposable : IDisposable
	{
		public static readonly EmptyDisposable Instance = new();

		public void Dispose()
		{
		}
	}

	private class ActionDisposable : IDisposable
	{
		private readonly Action _action;

		public ActionDisposable(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			_action();
		}
	}
}