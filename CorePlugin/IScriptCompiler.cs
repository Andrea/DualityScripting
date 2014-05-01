﻿using System.Reflection;

namespace ScriptingPlugin
{
	public enum CompilerResult
	{
		/// <summary>
		/// Either the assembly was succesfuly created or it exists
		/// </summary>
		AssemblyExists,
		CompilerError,
		PdbEditorError,
		GeneralError
	}

	public interface IScriptCompiler
	{
		CompilerResult TryCompile(string scriptName, string scriptPath, string script,out Assembly assembly);
	}
}