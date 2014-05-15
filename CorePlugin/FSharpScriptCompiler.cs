﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;

namespace ScriptingPlugin
{
	public class FSharpScriptCompiler : IScriptCompiler
	{
		private List<string> _references = new List<string>();
		private SimpleSourceCodeServices _sourceCodeServices;

		public FSharpScriptCompiler()
		{
			_sourceCodeServices = new SimpleSourceCodeServices();
		}

		public CompilerResults Compile(string script)
		{
			var outputAssemblyPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetTempFileName() + ".dll");
			var referencesAndScript = new List<string>();

			foreach (var reference in _references)
				referencesAndScript.Add(string.Format("--reference:{0}", reference));

			var options = new[] { "fsc.exe", "-o", outputAssemblyPath, "-a", "-g", "--lib:plugins", "--noframework" };

			var tempScriptPath = "";
			try
			{
				tempScriptPath = Path.GetTempFileName();
				File.WriteAllText(tempScriptPath, script);

				referencesAndScript.Add(tempScriptPath);
				var completeOptions = options.Concat(referencesAndScript).ToArray();
				var errorsAndExitCode = _sourceCodeServices.Compile(completeOptions);

				Assembly assembly = null;
				try
				{
					assembly = Assembly.LoadFile(outputAssemblyPath);
				}
				catch (Exception e)
				{
					Log.Editor.WriteWarning("{0}: Couldn't load assembly file", GetType());
				}

				var compilerResults = new CompilerResults(new TempFileCollection()) {CompiledAssembly = assembly, PathToAssembly = outputAssemblyPath};
				ErrorInfoToCompilerErrors(errorsAndExitCode, compilerResults);
				return compilerResults;
			}
			finally
			{
				if (File.Exists(tempScriptPath))
					File.Delete(tempScriptPath);
			}
		}

		private static void ErrorInfoToCompilerErrors(Tuple<ErrorInfo[], int> errorsAndExitCode, CompilerResults compilerResults)
		{
			foreach (var errorInfo in errorsAndExitCode.Item1)
			{
				compilerResults.Errors.Add(new CompilerError(errorInfo.FileName, errorInfo.StartLineAlternate,
					errorInfo.StartColumn, errorInfo.Subcategory, errorInfo.Message));
			}
		}

		public void AddReference(string referenceAssembly)
		{
			_references.Add(referenceAssembly);
		}
	}
}