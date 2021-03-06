using System;
using System.Collections;
using NHibernate.Util;

namespace NHibernate.Engine.Query.Sql
{
	/// <summary> 
	/// Represents the base information for a non-scalar return defined as part of
	/// a native sql query. 
	/// </summary>
	[Serializable]
	public abstract class NativeSQLQueryNonScalarReturn : INativeSQLQueryReturn
	{
		private readonly string alias;
		private readonly Hashtable propertyResults = new Hashtable();
		private readonly LockMode lockMode;

		/// <summary> Constructs some form of non-scalar return descriptor </summary>
		/// <param name="alias">The result alias </param>
		/// <param name="propertyResults">Any user-supplied column->property mappings </param>
		/// <param name="lockMode">The lock mode to apply to the return. </param>
		protected internal NativeSQLQueryNonScalarReturn(string alias, IDictionary propertyResults, LockMode lockMode)
		{
			if (string.IsNullOrEmpty(alias))
				throw new ArgumentNullException("alias", "A valid scalar alias must be specified.");

			this.alias = alias;
			this.lockMode = lockMode;
			
			if (propertyResults != null)
			{
				ArrayHelper.AddAll(this.propertyResults, propertyResults);
			}
		}

		/// <summary> Retrieve the defined result alias </summary>
		public string Alias
		{
			get { return alias; }
		}

		/// <summary> Retrieve the lock-mode to apply to this return </summary>
		public LockMode LockMode
		{
			get { return lockMode; }
		}

		/// <summary> Retrieve the user-supplied column->property mappings. </summary>
		public Hashtable PropertyResultsMap
		{
			get { return propertyResults; }
		}
	}
}