using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycWpfLibrary.Xml;

namespace WpfNpuzzle
{
  public class XmlRecord
  {
    public SerializableDictionary<int, int?> dictionary { get; set; } = new SerializableDictionary<int, int?>
    {
      { 3, null },
      { 4, null },
      { 5, null },
      { 6, null }
    };

  }
}
