using NHibernate.Properties;

namespace NHibernate.Bytecode
{
	/// <summary>
	/// A <see cref="IBytecodeProvider" /> implementation that returns
	/// <see langword="null" />, disabling reflection optimization.
	/// </summary>
	public class NullBytecodeProvider : IBytecodeProvider
	{
		#region IBytecodeProvider Members

		public IProxyFactoryFactory ProxyFactoryFactory
		{
			get { return new DefaultProxyFactoryFactory(); }
		}

		#endregion

		public IReflectionOptimizer GetReflectionOptimizer(System.Type clazz, IGetter[] getters, ISetter[] setters)
		{
			return null;
		}
	}
}