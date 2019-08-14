using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wpf15puzzle
{
  /// <summary>
  /// 將所有Dll檔合併到exe中，已捨棄
  /// </summary>
  class Program
  {
    [STAThreadAttribute]
    public static void Main()
    {
      AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
      App.Main();
    }

    private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
    {
      var executingAssembly = Assembly.GetExecutingAssembly();
      var assemblyName = new AssemblyName(args.Name);
      
      var path = assemblyName.Name + ".dll";
      if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false) path =
        $@"{assemblyName.CultureInfo}\{path}";

      using (var stream = executingAssembly.GetManifestResourceStream(path))
      {
        if (stream == null)
          return null;

        var assemblyRawBytes = new byte[stream.Length];
        stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
        return Assembly.Load(assemblyRawBytes);
      }
    }
  }
}
