using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSettings
{
	public enum PersistenceType
	{
		ReadOnly, ReadWrite
	}

	public readonly string dataTag;
	public readonly PersistenceType persistenceType;

	public DataSettings(PersistenceType persistenceType)
	{
		dataTag = System.Guid.NewGuid().ToString();
		this.persistenceType = persistenceType;
	}

	public override string ToString()
	{
		return dataTag + " " + persistenceType.ToString();
	}
}

public class Data
{

}

public class Data<T> : Data
{
	public T Value;

	public Data(T value)
	{
		Value = value;
	}
}

public class Data<T0, T1> : Data
{
	public T0 Value0;
	public T1 Value1;

	public Data(T0 value0, T1 value1)
	{
		Value0 = value0;
		Value1 = value1;
	}
}

public class Data<T0, T1, T2> : Data
{
	public T0 Value0;
	public T1 Value1;
	public T2 Value2;

	public Data(T0 value0, T1 value1, T2 value2)
	{
		Value0 = value0;
		Value1 = value1;
		Value2 = value2;
	}
}

public class Data<T0, T1, T2, T3> : Data
{
	public T0 Value0;
	public T1 Value1;
	public T2 Value2;
	public T3 Value3;

	public Data(T0 value0, T1 value1, T2 value2, T3 value3)
	{
		Value0 = value0;
		Value1 = value1;
		Value2 = value2;
		Value3 = value3;
	}
}

public class Data<T0, T1, T2, T3, T4>: Data
{
	public T0 Value0;
	public T1 Value1;
	public T2 Value2;
	public T3 Value3;
	public T4 Value4;

	public Data(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4)
	{
		Value0 = value0;
		Value1 = value1;
		Value2 = value2;
		Value3 = value3;
		Value4 = value4;
	}
}
