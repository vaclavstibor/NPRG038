using System.Reflection;

namespace LinqToMinimalSql {

	class SqlTable<T> : IEnumerable<T> where T : new() {
		// Client side data:
		FieldInfo[] columnFields;

		internal readonly Dictionary<string, string> FieldNameToColumnNameMap = new Dictionary<string, string>();

		// DB server side data:
		string[] columnNames;
		string[][] data;

		public SqlTable(string[] columnNames, string[][] data) {
			// DB server side initialization
			this.columnNames = columnNames;
			this.data = data;


			// Client side initialization
			var fieldInfos = typeof(T).GetFields();

			columnFields = new FieldInfo[columnNames.Length];
			for (int i = 0; i < columnFields.Length; i++) {
				columnFields[i] = FindFieldInfoForDatabaseColumn(columnNames[i], fieldInfos);
				FieldNameToColumnNameMap.Add(columnFields[i].Name, columnNames[i]);
			}
		}

		private static FieldInfo FindFieldInfoForDatabaseColumn(string columnName, FieldInfo[] fieldInfos) {
			columnName = new string(columnName.Where(c => c != '_').ToArray());

			foreach (var fi in fieldInfos) {
				if (StringComparer.InvariantCultureIgnoreCase.Compare(fi.Name, columnName) == 0) {
					return fi;
				}
			}

			return null;
		}

		public IEnumerator<T> GetEnumerator() {
			Console.WriteLine("  + SqlTable<{0}>.GetEnumerator() called ...", typeof(T).Name);

			foreach (string[] row in data) {	// This line simulates fetching data from a remote database server.
				T t = new T();

				for (int i = 0; i < row.Length; i++) {
					columnFields[i].SetValue(t, row[i]);
				}

				Console.WriteLine("  + SqlTable<{0}>.Enumerator.MoveNext() called, .Current <- {1} ...", typeof(T).Name, t);
				yield return t;
			}

			Console.WriteLine("  + SqlTable<{0}>.GetEnumerator() finished ...", typeof(T).Name);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	class Person {
		public string FirstName;
		public string LastName;

		public override string ToString() {
			return string.Format("Person(FirstName = {0}, LastName = {1})", FirstName, LastName);
		}
	}

	class Program {
		static void Main(string[] args) {
			string[] columns = { "LAST_NAME", "FIRST_NAME" };
			string[][] data = {
								  new [] { "Jezek", "Pavel" },
								  new [] { "Kofron", "Jan" },
								  new [] { "Jezek", "Ota" },
								  new [] { "Vesely", "Jiri" },
								  new [] { "Adamek", "Jiri" },
								  new [] { "Vesely", "Petr" },
								  new [] { "Humpolicek", "Jiri" }
							  };

			SqlTable<Person> persons = new SqlTable<Person>(columns, data);

			Console.WriteLine("Create query for LINQ to Objects ...");
			var q = from p in persons orderby p.LastName where p.FirstName == "Jiri" select p;
			
			Console.WriteLine("LINQ to Objects:");
			foreach (var p in q) {
				Console.WriteLine("\t{0}", p);
			}
		}
	}
}
