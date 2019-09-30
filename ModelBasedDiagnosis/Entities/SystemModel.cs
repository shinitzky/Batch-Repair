using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class SystemModel
    {
        public string Id { get; private set; }
        public List<Wire> Internal;
        public List<Wire> Input;
        public List<Wire> Output;
        public List<Gate> Components { get; private set; }
        public List<Cone> Cones { get; private set; }
        public List<Gate> InputComponents { get; private set; }
        public List<Gate> OutputComponents { get; private set; }
        public SystemModel(string id)
        {
            Id = id;
            Input = new List<Wire>();
            Output = new List<Wire>();
            Components = new List<Gate>();
            Cones = new List<Cone>();
            Internal = new List<Wire>();
            InputComponents = new List<Gate>();
            OutputComponents = new List<Gate>();
        }
        public SystemModel(string id, Wire[] input, Wire[] output) : this(id)
        {
            //Id = id;
            if (input == null)
                Input = new List<Wire>();
            else
                Input = input.ToList();
            if (output == null)
                Output = new List<Wire>();
            else
                Output = output.ToList();
        }
        public SystemModel(string id, List<Wire> input, List<Wire> output)
            : this(id)
        {
            Input = input;
            Output = output;
        }
        public void AddComponent(Gate Component)
        {
            if (Component != null)
            {
                Components.Add(Component);
                if (Component.Output.Type == Wire.WireType.o)
                    OutputComponents.Add(Component);
                bool isInComponent = true;
                if (Component is MultipleInputComponent)
                {
                    foreach (Wire w in ((MultipleInputComponent)Component).Input)
                    {
                        if (w.Type != Wire.WireType.i)
                        {
                            isInComponent = false;
                            break;
                        }
                    }
                }
                else if (Component is OneInputComponent)
                {
                    if (((OneInputComponent)Component).Input1.Type != Wire.WireType.i)
                        isInComponent = false;
                }
                else if (Component is Cone)
                {
                    foreach (Wire w in ((Cone)Component).cone.Input)
                    {
                        if (w.Type != Wire.WireType.i)
                        {
                            isInComponent = false;
                            break;
                        }
                    }
                }
                if (isInComponent)
                    InputComponents.Add(Component);
            }
        }
        public bool[] GetValue()
        {
            bool[] ans = new bool[Output.Count];
            for (int i = 0; i < Output.Count; i++)
            {
                ans[i] = Output[i].Value;
            }
            return ans;
        }

        public void SetValue(bool[] inputVals)
        {
            if (inputVals.Length != Input.Count)
                return; //throw exception
            for (int i = 0; i < Input.Count; i++)
            {
                Input[i].Value = inputVals[i];
            }
        }
        public void SortComponents()
        {
            if (Components.Count == 0)
                return;
            Stack<Gate> sorted = new Stack<Gate>();
            foreach (Wire wire in Output)
            {
                sorted.Push(wire.InputComponent);
            }
            List<Gate> list = sorted.ToList();
            while (list != null && list.Count != 0)
            {
                List<Gate> temp = new List<Gate>();
                foreach (Gate g in list)
                {
                    if (g is MultipleInputComponent)
                    {
                        foreach (Wire w in ((MultipleInputComponent)g).Input)
                        {
                            if (w.InputComponent != null)
                            {
                                sorted.Push(w.InputComponent);
                                temp.Add(w.InputComponent);
                            }
                        }
                    }
                    else if (g is OneInputComponent)
                    {
                        if (((OneInputComponent)g).Input1.InputComponent != null)
                        {
                            sorted.Push(((OneInputComponent)g).Input1.InputComponent);
                            temp.Add(((OneInputComponent)g).Input1.InputComponent);
                        }
                    }
                }
                list = temp;
            }

            int order = 1;
            while (sorted.Count != 0)
            {
                Gate g = sorted.Pop();
                if (g != null && !list.Contains(g))
                {
                    list.Add(g);
                    g.Order = order;
                    order++;
                }
            }
            if (list.Count != Components.Count)
            {
                Console.WriteLine("Model Sort Component Error");
                return;
            }
            foreach (Gate g in Components)
            {
                if (!list.Contains(g))
                {
                    Console.WriteLine("Model Sort Component Error");
                    return;
                }
            }
            Components = list;
        }

        public void createCones()
        {
            if (Cones == null || Cones.Count > 0)
                return;
            Dictionary<int, Cone> ComponentConeDic = new Dictionary<int, Cone>(); //int Component_id-> Model cone
            foreach (Wire wire in Output)
            {
                Cone cone = new Cone(wire.InputComponent.Id);
                cone.Output = wire;
                cone.Order = wire.InputComponent.Order;
                cone.cone.AddComponent(wire.InputComponent); //if wire.InputComponent==null?
                ComponentConeDic.Add(wire.InputComponent.Id, cone);
                Cones.Add(cone);
            }

            foreach (Wire wire in Input)
            {
                if (wire.OutputComponents == null || wire.OutputComponents.Count == 0)
                    continue;
                foreach (Gate Component in wire.OutputComponents)
                {
                    createConesRec(ComponentConeDic, Component);
                    if (!ComponentConeDic.ContainsKey(Component.Id))
                    {
                        Cone newcone = new Cone(Component.Id);
                        newcone.Output = Component.Output;
                        Cones.Add(newcone);//
                        ComponentConeDic.Add(Component.Id, newcone);
                        newcone.cone.AddComponent(Component);
                        newcone.Order = Component.Order;
                        if (Component is MultipleInputComponent)
                            ComponentConeDic[Component.Id].cone.Input.AddRange(((MultipleInputComponent)Component).Input);
                        else if (Component is OneInputComponent)
                            ComponentConeDic[Component.Id].cone.Input.Add(((OneInputComponent)Component).Input1);
                    }
                    else
                        ComponentConeDic[Component.Id].cone.Input.Add(wire);
                }
            }

            //cones input
            foreach (Cone cone in Cones)
            {
                if (cone.Output.OutputComponents != null && cone.Output.OutputComponents.Count != 0)//cone with inside output
                {
                    foreach (Gate g in cone.Output.OutputComponents)
                    {
                        if (ComponentConeDic.ContainsKey(g.Id))
                        {
                            ComponentConeDic[g.Id].cone.Input.Add(cone.Output);
                        }

                    }
                }
            }
            Cones.Sort(Gate.CompareComponents);
            foreach (Cone cone in Cones)
            {
                cone.SetInputOutputComponents();
            }
        }
        private void createConesRec(Dictionary<int, Cone> ComponentConeDic, Gate Component)
        {
            if (ComponentConeDic.ContainsKey(Component.Id))
                return;
            Cone cone = null;
            if (Component.Output.OutputComponents == null || Component.Output.OutputComponents.Count == 0)
                return;
            foreach (Gate g in Component.Output.OutputComponents)
            {
                createConesRec(ComponentConeDic, g);
                if (ComponentConeDic.ContainsKey(g.Id))
                {
                    if (cone == null)
                    {
                        cone = ComponentConeDic[g.Id];
                    }
                    else if (cone.Id != ComponentConeDic[g.Id].Id)
                    {
                        cone = new Cone(Component.Id);
                        cone.Output = Component.Output;
                        Cones.Add(cone);//
                        cone.Order = Component.Order;
                        break;
                    }
                }
            }
            if (cone != null)
            {
                ComponentConeDic.Add(Component.Id, cone);
                cone.cone.AddComponent(Component);
            }

        }

        public Dictionary<int, Gate> CreateCompDic()
        {
            Dictionary<int, Gate> compDic = new Dictionary<int, Gate>();
            if (Components == null || Components.Count == 0)
                return compDic;
            foreach (Gate g in Components)
            {
                if (!compDic.ContainsKey(g.Id))
                    compDic.Add(g.Id, g);
            }
            return compDic;
        }
    }
}
