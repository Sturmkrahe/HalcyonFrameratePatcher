using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace HalcyonFrameratePatcher
{
    class PatcherProgram
    {
        const string FILE_PATH_DEFAULT = @"C:\Program Files (x86)\Steam\steamapps\common\Halcyon 6 Starbase Commander\H6_Data\Managed\Assembly-CSharp.dll";
        static string filePath;

        static void Main(string[] args)
        {
            Console.WriteLine("Do not use this while the game is running!");

            try
            {
                PatchGame();
                Console.WriteLine("\nFinished patching. You may now close this window.");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nThere was an issue patching the game.");
                Console.WriteLine(e.Message);

            }

            System.Console.ReadLine();
        }

        static void PatchGame()
        {

            string targetOpCodeFirst = "ldc.i4.s";
            string targetOperandSecond = "set_targetFrameRate";

            Console.WriteLine("\nEnter the path to \"Assembly-CSharp.dll\" (or leave empty to use the default location of \"" + FILE_PATH_DEFAULT + "\"): ");
            filePath = Console.ReadLine();
            if (filePath == "")
            {
                filePath = FILE_PATH_DEFAULT;
            }

            AssemblyDefinition gameAssembly = null;
            try
            {
                gameAssembly = AssemblyDefinition.ReadAssembly(filePath);
            }
            catch
            {
                throw;
            }

            TypeDefinition masterControllerType = gameAssembly.MainModule.GetType("Mdi.H6.UI.GameMasterController");
            if (masterControllerType == null)
            {
                throw new Exception("Code module could not be located.");
            }

            MethodDefinition foundInitializeMethod;
            try
            {
                foundInitializeMethod = masterControllerType.Methods.OfType<MethodDefinition>().Where(method => method.Name == "Initialize").Single();
            }
            catch
            {
                throw new Exception("Initialization function could not be located.");
            }

            if (foundInitializeMethod == null)
            {
                throw new Exception("Initialization function could not be located.");
            }

            ILProcessor processor = foundInitializeMethod.Body.GetILProcessor();
            if (processor == null)
            {
                throw new Exception("IL processor error.");
            }


            Instruction framerateSetInstr = foundInitializeMethod.Body.Instructions.OfType<Instruction>().Where(instr => instr.OpCode.ToString() == targetOpCodeFirst).Single();
            if (framerateSetInstr == null)
            {
                throw new Exception("First OpCode not found.");
            }

            var nextOperand = framerateSetInstr.Next.Operand;
            if (nextOperand == null)
            {
                throw new Exception("Second OpCode operand not found.");
            }

            if (!(nextOperand is MethodReference))
            {
                throw new Exception("Second operand is not a method reference.");
            }

            if (((MethodReference)nextOperand).Name != targetOperandSecond)
            {
                throw new Exception("Second operand is not the expected method reference.");
            }

            int targetFPS = 60;
            Console.Write("\nEnter your preferred framerate: ");

            try
            {
                targetFPS = int.Parse(Console.ReadLine());
            }
            catch
            {
                throw;
            }

            if (targetFPS < 5 || targetFPS > 500)
            {
                throw new Exception("Invalid target framerate.");
            }

            Console.WriteLine("Setting target framerate to: " + targetFPS);
            framerateSetInstr.Operand = (sbyte)targetFPS;

            try
            {
                gameAssembly.Write(filePath);
            }
            catch
            {
                throw;
            }
        }
    }
}
