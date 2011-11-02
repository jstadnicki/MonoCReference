using System.Linq;

namespace mcWorker
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    class Program
    {
        static void Main(string[] args)
        {
            string sourceExe = @"content\SourceForm.exe";
            string targetExe = @"content\TargetForm.exe";

            string srcMethod1 = "Form1_KeyDown";
            string srcMethod2 = "Form1_KeyUp";

            // Source of code
            AssemblyDefinition adSrc = AssemblyDefinition.ReadAssembly(sourceExe);
            TypeDefinition typeSrc = adSrc.MainModule.Types.Where(t => t.Name == "SourceForm").First();


            // Place where code will be added
            AssemblyDefinition adDst = AssemblyDefinition.ReadAssembly(targetExe);
            TypeDefinition typeDst = adDst.MainModule.Types.Where(t => t.Name == "TargetForm").First();

            // Copy both methods from src to dst
            MethodDefinition m1 = CopyMethod(adSrc, typeSrc, srcMethod1, adDst, typeDst);
            MethodDefinition m2 = CopyMethod(adSrc, typeSrc, srcMethod2, adDst, typeDst);

            // Now they should be marked as event handlers
            AddEventHandlers(adDst, typeDst, "KeyDown", m1);
            AddEventHandlers(adDst, typeDst, "KeyUp", m2);

            adDst.Write(@"newTarget.exe");

        }

        private static void AddEventHandlers(AssemblyDefinition adDestination, TypeDefinition typeDestination, string aEventName, MethodDefinition aEventHandler)
        {
            // For the simpliciyty of code the event handler will be connected to the events in default constructors
            // Just before leaving it

            // I know that there is just one constructor, but this is only an example!
            var ctor = typeDestination.Methods.Where(m => m.IsConstructor).First();

            // Find last return code
            // We will put our code just before that opcode
            var lastRet = ctor.Body.Instructions.Reverse().Where(i => i.OpCode == OpCodes.Ret).First();

            // Now we need an IL generator
            var ilg = ctor.Body.GetILProcessor();

            // and now the magic
            ilg.InsertBefore(lastRet, Instruction.Create(OpCodes.Ldarg_0));
            ilg.InsertBefore(lastRet, Instruction.Create(OpCodes.Ldarg_0));
            ilg.InsertBefore(lastRet, Instruction.Create(OpCodes.Ldftn, aEventHandler));

            // I did check here also that there is only one construcor
            ilg.InsertBefore(
                lastRet,
                Instruction.Create(
                    OpCodes.Newobj,
                    adDestination.MainModule.Import(typeof(System.Windows.Forms.KeyEventHandler).GetConstructors().First())));

            ilg.InsertBefore(
                lastRet,
                Instruction.Create(
                    OpCodes.Callvirt,
                    adDestination.MainModule.Import(typeof(System.Windows.Forms.Control).GetEvent(aEventName).GetAddMethod())));
        }

        private static MethodDefinition CopyMethod(AssemblyDefinition adSource, TypeDefinition typeSource, string mthdName, AssemblyDefinition adDestination, TypeDefinition typeDestination)
        {
            // source 
            MethodDefinition srcMethod = typeSource.Methods.Where(m => m.Name == mthdName).First();

            // now create a new place holder for copy
            MethodDefinition target = new MethodDefinition(srcMethod.Name, srcMethod.Attributes, adDestination.MainModule.Import(srcMethod.ReturnType));

            // Copy all method parameters
            // I could use var, but I did this on purpose to show the type used.
            foreach (ParameterDefinition pd in srcMethod.Parameters)
            {
                target.Parameters.Add(new ParameterDefinition(pd.Name, pd.Attributes, adDestination.MainModule.Import(pd.ParameterType)));
            }

            // Now copy all local variables that are defined withing method body
            // I could use var, but I did this on purpose to show the type used.
            foreach (VariableDefinition vd in srcMethod.Body.Variables)
            {
                target.Body.Variables.Add(new VariableDefinition(adDestination.MainModule.Import(vd.VariableType)));
            }

            // copy the state
            target.Body.InitLocals = srcMethod.Body.InitLocals;

            /* copy all instructions from SRC to DST */
            foreach (Instruction instruction in srcMethod.Body.Instructions)
            {
                MethodReference mr = instruction.Operand as MethodReference;    // Case when method call another method defined withing SRC type/assembly
                FieldReference fr = instruction.Operand as FieldReference;      // Case when method load field from type/assembly
                TypeReference tr = instruction.Operand as TypeReference;
                if (mr != null)
                {
                    if (mr.DeclaringType == typeSource)
                    {
                        // That would mean that here we have a method call to method within source type
                        // And this need to be redirected to source type or handled in some other way
                        // But in this example is not used
                        // If you want some examples please contace me
                    }
                    else
                    {
                        target.Body.Instructions.Add(
                            Instruction.Create(instruction.OpCode, adDestination.MainModule.Import(mr)));
                    }
                }
                else
                {
                    if (fr != null)
                    {
                        // So we migth found our selfs in position that we need to redirect this load to some other field or remove it.
                        // For now lets redirect for different field
                        // Please try to remove the code between TRY ME and check what peverify.exe will tell
                        /*TRY ME*/
                        if (fr.Name == "sourceStatus")
                        {
                            target.Body.Instructions.Add(
                                Instruction.Create(
                                    instruction.OpCode,
                                    adDestination.MainModule.Import(typeDestination.Fields.Where(f => f.Name == "targetStatus").First())));
                        }
                        else/*TRY ME*/
                        {
                            target.Body.Instructions.Add(Instruction.Create(instruction.OpCode, adDestination.MainModule.Import(fr)));
                        }

                    }
                    else if (tr != null)
                    {
                        target.Body.Instructions.Add(Instruction.Create(instruction.OpCode, adDestination.MainModule.Import(tr)));
                    }
                    else
                    {
                        target.Body.Instructions.Add(instruction);
                    }
                } // else
            } // foreach

            typeDestination.Methods.Add(target);
            return target;
        }
    }
}
