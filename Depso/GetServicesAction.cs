using System;
using System.Collections.Generic;

namespace Depso;

public class GetServicesAction : IComparable<GetServicesAction>, IComparable
{
	public const int OrderSingleton = 10000000;
	public const int OrderScoped = 20000000;
	public const int OrderTransient = 30000000;
	public const int OrderEnumerables = 40000000;
	public const int OrderMedi = 50000000;

	public Action<GenerationContext> Action { get; }
	public int Order { get; }

	public GetServicesAction(Action<GenerationContext> action, int order)
	{
		Action = action;
		Order = order;
	}

	public int CompareTo(GetServicesAction? other)
	{
		if (ReferenceEquals(this, other)) return 0;
		if (ReferenceEquals(null, other)) return 1;
		return Order.CompareTo(other.Order);
	}

	public int CompareTo(object? obj)
	{
		if (ReferenceEquals(null, obj)) return 1;
		if (ReferenceEquals(this, obj)) return 0;
		return obj is GetServicesAction other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(GetServicesAction)}");
	}

	public static bool operator <(GetServicesAction? left, GetServicesAction? right)
	{
		return Comparer<GetServicesAction?>.Default.Compare(left, right) < 0;
	}

	public static bool operator >(GetServicesAction? left, GetServicesAction? right)
	{
		return Comparer<GetServicesAction?>.Default.Compare(left, right) > 0;
	}

	public static bool operator <=(GetServicesAction? left, GetServicesAction? right)
	{
		return Comparer<GetServicesAction?>.Default.Compare(left, right) <= 0;
	}

	public static bool operator >=(GetServicesAction? left, GetServicesAction? right)
	{
		return Comparer<GetServicesAction?>.Default.Compare(left, right) >= 0;
	}
}