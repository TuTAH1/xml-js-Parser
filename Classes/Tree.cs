using Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium;
using static xml_js_Parser.Classes.Methods;

namespace xml_js_Parser.Classes
{
	
	public class TreeNode<T> : IEnumerable
	{
		public T? Value;
		public List<TreeNode<T>> Childs;
		public TreeNode<T>? Parent;
		public int Count => Childs.Count;
		public bool Empty => Childs.Count == 0;

		public TreeNode(T? Value = default)
		{
			this.Value = Value;
			this.Childs = new List<TreeNode<T>>();
		}

		public TreeNode<T> this[int index] => Childs[index];

		/// <summary>
		/// Adds a TreeNode as a child to the current node
		/// </summary>
		/// <param name="Name"></param>
		/// <returns>Added node</returns>
		public TreeNode<T> Add(T? Value = default)
		{
			var node = new TreeNode<T>(this, Value);
			Childs.Add(node);
			return node;
		}

		/// <summary>
		/// Adds a TreeNode as a child to the current node
		/// </summary>
		/// <param name="Name"></param>
		/// <returns>Added node</returns>
		public TreeNode<T> Add(TreeNode<T> Node)
		{
			Node.Parent = this;
			Childs.Add(Node);
			return Node;
		}

		/// <summary>
		/// Adds a TreeNodes as a childs to the current node
		/// </summary>
		/// <param name="Name"></param>
		/// <returns>Added node</returns>
		public IEnumerable<TreeNode<T>> Add(IEnumerable<TreeNode<T>> Nodes)
		{
			foreach (var node in Nodes)
			{
				this.Add(node);
			}

			return Nodes;
		}

		/// <summary>
		/// Deletes current node
		/// </summary>
		/// <param name="Recursive">Also delete all childs and subchilds</param>
		/// <returns>Node's Parent</returns>
		public TreeNode<T> Delete(bool Recursive = false)
		{
			if (this.Empty)
			{
				this.Parent.Childs.Remove(this);
			}
			else if (Recursive) 
				this.Childs.ForEach(x=> x.Delete(true));
			else if (Parent != null)
			{
				this.Parent.Childs.Remove(this); //: Сборщик мусора должен dispose it за меня.
				this.Parent.Childs.AddRange(this.Childs);
			}
			else throw new ArgumentException("Root node can't be deleted");

			return this.Parent;
		}

		public bool DeleteWithNoValue(bool IsRootNode = true)
		{
			if (!IsRootNode && Value == null)
			{
				Delete();
				return true;
			}

			for (var i = 0; i < Childs.Count;)
			{
				var child = Childs[i];
				if (!child.DeleteWithNoValue(false)) i++;
			}

			return false;
		}

		private TreeNode(TreeNode<T> Parent, List<TreeNode<T>> Childs, T? Value = default)
		{
			this.Childs = Childs;
			this.Value = Value;
			this.Parent = Parent;
		}

		private TreeNode(TreeNode<T> Parent, T? Value = default)
		{
			this.Value = Value;
			this.Parent = Parent;
			this.Childs = new List<TreeNode<T>>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Childs.GetEnumerator();
		}
	}

	
	public static class TreeFuncs
	{
		/// <summary>
		/// Создаёт потомков внутри дерева, а также потмков внутри созданных потомков, глубина равна длине ValueTypes
		/// </summary>
		/// <param name="Tree">Дерево-источник</param>
		/// <param name="Types"></param>
		/// <param name="ValueTypes"></param>
		/// <param name="TargetTree">Дерево, куда помещаются найденные потомки (по умолчанию, Tree)</param>
		/// <param name="recursive">Искать потомков внутри дерево по данным Types[i] и ValueTypes[i], пока они находятся (далее идёт следующий Types). По умолчанию, все false</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static IEnumerable<TreeNode<Data>>? 
			CreateChilds(this TreeNode<Data> Tree, XMLData[] NodeData, TreeNode<Data>? TargetTree = null)
		{
			if (NodeData.Length < 1) return null;

			bool targetingEmptyNode = NodeData[0].NameType == null && NodeData[0].ValueType == null;
			int start = NodeData[0].Recursive ? 0 : 1;
			var TreeName = Tree.Value.Code;
			var Nodes = Tree.Value.xml.Elements(Name(NodeData[0].Type));
			//if(!Nodes.ToArray().Any()) {ReWrite(new []{"\nЭлемент ",TreeName," не содержит потомков"}, new []{c.gray, c.white,c.gray}); return null;}
		
			int i = 0;
			foreach (var obj in Nodes)
			{
				i++;
				var objCode = NodeData[0].NameType == null? null : obj.Element(Name(NodeData[0].NameType))?.Value.RemoveFrom(TypesFuncs.Side.End, "_a", "Auto");
				if(objCode?.EndsWith("Editable") == true) continue;
				var objValue = NodeData[0].ValueType == null? null : obj.Element(Name(NodeData[0].ValueType))?.Value;
				var tableRow = Program.Dictionary.GetByCode(objCode);
				//var critical = objCode == null && objValue == null;
				if (Program.SkipList.Contains((objCode,false))) tableRow = null;
				else if (tableRow == null && NodeData[0].AskIfNotFound) Program.Dictionary.AskName(objCode);

				var leave = new TreeNode<Data>(new Data(obj, tableRow?.Code, tableRow?.Text.Escape(@"'""\"), tableRow?.Optional, objValue, tableRow==null)); //: эскапирование кавычек. Правиль

				/*if (NodeData[0].AlwaysSkip || tableRow==null ||(critical&&targetingEmptyNode))
				{
					Skip();
					continue;
				} */
				(TargetTree ?? Tree).Add(leave);

				/*void Skip()
				{
					if (leave.CreateChilds(NodeData[start..], Tree) == null)  //: Универсальнее было бы ставить метку на удаление (узла без потомков)
						NodeData[0].Recursive = false;
				}*/

				if (leave.CreateChilds(NodeData[start..]) == null)
					NodeData[0].Recursive = false;
			}



			//if (Recursive) return Nodes.ToList().ForEach(x => x.Elements(Type).CreateTree(Tr))
			return Tree.Childs;
		}
	}

}
