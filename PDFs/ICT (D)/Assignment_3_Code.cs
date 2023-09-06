using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.Linq.Expressions;

namespace Project
{
    public class PostfixToInfixConverter
    {
        protected readonly static Logs logs = new Logs();


        // Entry point of the program
        static void Main(string[] args)
        {
            while (true)
            {
                ASCII_Calculator("Your input: ");  // Display ASCII calculator header and prompt for user input

                string Input = Console.ReadLine().ToLower();  // Read user input and convert it to lowercase
                string Function = "";

                //Ensure that if a wrong input is made at any of the options, the error won't crash the application
                try
                {
                    switch (Input)
                    {
                        case "z":
                            ASCII_Calculator("Expression: ");  // Prompt the user for an expression
                            Function = FixUserInput(Console.ReadLine());  // Sanitize the input

                            Queue<string> queue = ShuntingYard(Function);  // Convert infix to postfix using the Shunting Yard algorithm
                            string Postfix = ToString(queue);  // Convert the resulting queue to a string
                            string Evaluation = PostfixEvaluation(ShuntingYard(Function));  // Evaluate the postfix expression

                            logs.AddLog(Function, Postfix, Evaluation);  // Add the input and results to the log

                            RedTitle("The postfix expression: ", Postfix + "\n");  // Display the postfix expression
                            RedTitle("The postfix evaluation: ", Evaluation + "\n");  // Display the postfix evaluation

                            RedTitle("Back [Y/N]: ", "");  // Prompt the user to continue or exit

                            if (Console.ReadLine().ToLower() == "n") Environment.Exit(0);  // If the user chooses to exit, terminate the program
                            else continue;  // Continue to the next iteration
                            break;

                        case "x":
                            ASCII_Calculator("Put multi-digit numbers within brackets\n");  // Display instructions for multi-digit numbers

                            RedTitle("Postfix notation: ", "");  // Prompt the user for a postfix expression
                            string PostfixExpression = Console.ReadLine().Trim();

                            string EvaluatedExpression = PostfixEvaluation(ToQueue(PostfixExpression));  // Evaluate the postfix expression

                            logs.AddLog("", PostfixExpression, EvaluatedExpression);  // Add the input and results to the log

                            RedTitle("The postfix expression: ", EvaluatedExpression + "\n");  // Display the postfix expression

                            RedTitle("Back [Y/N]: ", "");  // Prompt the user to continue or exit

                            if (Console.ReadLine().ToLower() == "n") Environment.Exit(0);  // If the user chooses to exit, terminate the program
                            else continue;  // Continue to the next iteration
                            break;

                        case "c":
                            ASCII_Calculator("");  // Display the ASCII calculator header and logs

                            logs.ShowLogs();  // Show usage logs

                            RedTitle("Back [Y/N]: ", "");  // Prompt the user to continue or exit

                            if (Console.ReadLine().ToLower() == "n") Environment.Exit(0);  // If the user chooses to exit, terminate the program
                            else continue;  // Continue to the next iteration
                            break;

                        default:
                            ASCII_Calculator("");  // Display the ASCII calculator header

                            Console.WriteLine("That's an invalid option");  // Show error message for invalid option

                            RedTitle("Back [Y/N]: ", "");  // Prompt the user to continue or exit

                            if (Console.ReadLine().ToLower() == "n") Environment.Exit(0);  // If the user chooses to exit, terminate the program
                            else continue;  // Continue to the next iteration
                            break;
                    }
                }
                catch
                {
                    ASCII_Calculator("");  // Display the ASCII calculator header

                    Console.WriteLine("The expression is invalid");  // Show error message for invalid input

                    RedTitle("Back [Y/N]: ", "");  // Prompt the user to continue or exit

                    if (Console.ReadLine().ToLower() == "n") Environment.Exit(0);  // If the user chooses to exit, terminate the program
                    else continue;  // Continue to the next iteration
                    break;

                }
            }
        }


        //Converts Infix expressions to Postfix and returns the output queue
        static Queue<string> ShuntingYard(string Expression)
        {
            List<string> SeperatedExpression = SplitExpression(Expression);
            Stack<string> stack = new Stack<string>();
            Queue<string> queue = new Queue<string>();

            foreach (var value in SeperatedExpression)
            {
                if (char.IsNumber(value.ToCharArray()[0]) || (char.IsLetter(value.ToCharArray()[0]) && value.Length == 1))
                {
                    queue.Enqueue(value);  // Enqueue numbers and single-letter variables
                }
                else
                {
                    if (stack.Count() == 0 || value == "(")
                    {
                        stack.Push(value);  // Push opening parentheses or operators onto the stack
                    }
                    else if (value == ")")
                    {
                        for (int i = 0; i < stack.Count(); i++)
                        {
                            if (stack.Peek().ToString() != "(")
                            {
                                queue.Enqueue(stack.Pop());  // Pop operators until opening parenthesis is found
                            }
                            else
                            {
                                stack.Pop();  // Pop the opening parenthesis

                                if (stack.Peek().ToString() == "sin" || stack.Peek().ToString() == "cos" || stack.Peek().ToString() == "tan" || stack.Peek().ToString() == "ln" || stack.Peek().ToString() == "log")
                                {
                                    queue.Enqueue(stack.Pop());  // Pop functions and enqueue them
                                }

                                break;
                            }
                        }
                    }
                    else if (BIDMAS(stack.Peek().ToString()) > BIDMAS(value.ToString()) || stack.Peek().ToString().Trim() == "(")
                    {
                        stack.Push(value);  // Push operators with higher precedence onto the stack
                    }
                    else
                    {
                        queue.Enqueue(stack.Pop());  // Pop operators with lower precedence and enqueue them
                        stack.Push(value);  // Push the current operator onto the stack
                    }
                }
            }

            if (stack.Count() != 0)
            {
                for (int i = 0; i < stack.Count(); i++)
                {
                    queue.Enqueue((stack.Peek() != "(") ? stack.Pop() : null);  // Pop any remaining operators and enqueue them
                }
            }

            return queue;  // Return the queue containing the output
        }


        //Evaluates the value of a given postfix expression
        static string PostfixEvaluation(Queue<string> queue)
        {
            List<string> SeperatedExpression = new List<string>();
            Stack<double> stack = new Stack<double>();

            int StartingCount = queue.Count();

            // Dequeue the items from the queue and store them in a list
            for (int i = 0; i < StartingCount; i++)
            {
                SeperatedExpression.Add(queue.Dequeue());
            }

            // Evaluate the postfix expression
            for (int i = 0; i < SeperatedExpression.Count(); i++)
            {
                string CurrentValue = SeperatedExpression[i];

                if ((!string.IsNullOrEmpty(CurrentValue)) ? char.IsNumber(CurrentValue.ToCharArray()[0]) || (char.IsLetter(CurrentValue.ToCharArray()[0]) && CurrentValue.Length == 1) : false)
                {
                    stack.Push(int.Parse(SeperatedExpression[i]));  // Push numbers onto the stack
                    continue;
                }

                switch (CurrentValue)
                {
                    case "*":
                        stack.Push(stack.Pop() * stack.Pop());  // Perform multiplication
                        break;
                    case "/":
                        double Divisor = stack.Pop();
                        double Dividend = stack.Pop();
                        stack.Push(Dividend / Divisor);  // Perform division
                        break;
                    case "^":
                        double Exponent = stack.Pop();
                        double Base = stack.Pop();
                        stack.Push((float)Math.Pow(Base, Exponent));  // Perform exponentiation
                        break;
                    case "+":
                        stack.Push(stack.Pop() + stack.Pop());  // Perform addition
                        break;
                    case "-":
                        double Subtrahend = stack.Pop();
                        double Minuend = stack.Pop();
                        stack.Push(Minuend - Subtrahend);  // Perform subtraction
                        break;
                    case "sin":
                        stack.Push(Math.Sin(stack.Pop()));  // Perform sine calculation
                        break;
                    case "cos":
                        stack.Push(Math.Cos(stack.Pop()));  // Perform cosine calculation
                        break;
                    case "tan":
                        stack.Push(Math.Tan(stack.Pop()));  // Perform tangent calculation
                        break;
                    case "log":
                        stack.Push(Math.Log(stack.Pop()));  // Perform logarithm calculation
                        break;
                    case "ln":
                        stack.Push(Math.Log(stack.Pop(), Math.E));  // Perform natural logarithm calculation
                        break;
                }
            }

            string Evaluation = "";
            int StackTotal = stack.Count();

            // Build the evaluation string by popping the items from the stack
            for (int i = 0; i < StackTotal; i++)
            {
                Evaluation += stack.Pop();
            }

            return Evaluation;  // Return the evaluated expression
        }


        // Checks the hierarchy of a given operator and returns an integer
        static int BIDMAS(string Operator)
        {
            
            List<string> Precedence = new List<string>() { "sin", "cos", "tan", "log", "ln", "(", "^", "/", "*", "+", "-" }; // Define the precedence of operators in descending order

            return (Precedence.Contains(Operator)) ? Precedence.IndexOf(Operator) : -1; // Return the index of the operator in the precedence list, or -1 if not found
        }


        // Splits the expression where there are functions or multi-digit numbers
        static List<string> SplitExpression(string Expression)
        {
            // Use regular expression to split the expression into individual tokens
            List<string> SplitExpression = Regex.Split(Expression, "(\\d{2,}|log|ln|sin|cos|tan|\\b\\w+\\b|[()+\\-*/^])(?!\\w)")
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return SplitExpression.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }


        // Used to convert a postfix expression into a usable queue
        static Queue<string> ToQueue(string Postfix)
        {
            List<string> Split = SplitExpression(Postfix);
            Queue<string> queue = new Queue<string>();

            for (int i = 0; i < Split.Count(); i++)
            {
                if (Split[i] == "(")
                {
                    // Combines the opening parantheses, number within, and closing parantheses into one index
                    Split[i] += Split[i + 1] + Split[i + 2];
                    Split.RemoveRange(i + 1, 2);
                }
                else if (Regex.IsMatch(Split[i], @"(log|ln|sin|cos|tan)|((?>!\()\d{2,}(?!\)))"))
                {
                    // Split the token further if it contains functions or multi-digit numbers
                    Split.InsertRange(i + 1, Regex.Split(Split[i], @"(log|ln|sin|cos|tan)|((?>!\()\d{2,}(?!\)))")
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
                    Split.RemoveAt(i);
                }
                else
                {
                    // If neither of the previous conditions apply, then the index is a multi-digit number that is NOT in brackets, for that it gets split into individual numbers
                    Split.InsertRange(i + 1, Split[i].ToCharArray().Select(x => x.ToString()));
                    Split.RemoveAt(i);
                }
            }

            // Remove parentheses and enqueue the tokens
            Split = Split.Select(s => s.Replace("(", "").Replace(")", "")).ToList();
            Split.ForEach(x => queue.Enqueue(x));
            return queue;
        }


        // Converts a queue of tokens back to a string representation of postfix expression
        static string ToString(Queue<string> queue)
        {
            string Postfix = "";
            int QueueTotal = queue.Count();

            for (int i = 0; i < QueueTotal; i++)
            {
                try
                {
                    // Check if the token is a multi-digit number and enclose it in parentheses
                    Postfix += (queue.Peek().All(x => char.IsNumber(x)) && queue.Peek().Length > 1)
                        ? "(" + queue.Dequeue().ToString() + ")"
                        : queue.Dequeue();
                }
                catch { }
            }

            return Postfix;
        }


        // Fixes the user input by applying necessary transformations
        static string FixUserInput(string Expression)
        {
            string FixedInput = Expression.Replace(" ", "").ToLower();

            // Replaces all spaces and converts the input to lowercase
            FixedInput = Expression.Replace(" ", "").ToLower();

            // Adds a multiplication operator (*) between a function name and an opening parenthesis
            FixedInput = Regex.Replace(FixedInput, @"(?<!\b(log|ln|sin|cos|tan))([A-Za-z]+)(\()", "$2$3");

            // Adds a multiplication operator (*) between a closing parenthesis and a function name
            FixedInput = Regex.Replace(FixedInput, @"(\))([A-Za-z]+)(?! *\()", "$1*$2");

            // Adds a multiplication operator (*) between a closing parenthesis and an opening parenthesis
            FixedInput = Regex.Replace(FixedInput, @"(\))(\()", "$1*$2");

            // Adds a multiplication operator (*) between a number and a letter
            FixedInput = Regex.Replace(FixedInput, "(\\d+)([a-zA-Z])", "$1*$2");

            // Adds a multiplication operator (*) between a letter and a number
            FixedInput = Regex.Replace(FixedInput, "([a-zA-Z])(\\d+)", "$2*$1");

            // Replaces the constant "e" with its numerical value
            FixedInput = FixedInput.Replace("e", Math.E.ToString());

            // Wraps the fixed input with parentheses to ensure proper evaluation
            return $"({FixedInput})";
        }


        //Starting screen including the main info about the program
        static void ASCII_Calculator(string Extra)
        {
            string Info = @"
_____________________
|  _________________  |                       
| | Infix - Postfix | |                            Overview of the program                                       Commands                                          Scope
| |_________________| |                     ______________________________________                    ______________________________                     ______________________________________
|  ___ ___ ___   ___  |                       
| | 7 | 8 | 9 | | + | |                     This program allows you to convert any                    (Z) -> Infix to postfix                            1. Utilize computer-based mathematical
| |___|___|___| |___| |                     infix-based mathematical expression to                                                                          operators (brackets, ^, /, *, +, -) 
| | 4 | 5 | 6 | | - | |                     postfix notation. A popular expression                    (X) -> Evaluate RPN expression           
| |___|___|___| |___| |                     format to increase computational speed                                                                       2. Trig functions and logarithm should 
| | 1 | 2 | 3 | | x | |                     and efficiency.                                           (C) -> Show usage logs                                be in short format (sin, not sine)
| |___|___|___| |___| |
| | . | 0 | = | | / | |                     The program also includes postfix eva-                                                                       3. Only log of base 10 is available
| |___|___|___| |___| |                     -luation for the converted expressions
|_____________________|                     with or without numeric variables                                                                            4. Variable are lowercase, e is unusable";

            
            //Clears the screen in order to not add clutter to the screen and then showcases the program's information
            Console.Clear(); 
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(Info + "\n\n\n");

            //Extra information to be added. Can be changed when calling the method
            Console.ForegroundColor = ConsoleColor.Red; 
            Console.Write(Extra);
            Console.ForegroundColor = ConsoleColor.White;
        }


        // Displays a title in red color followed by content in white color. Reduces code repetition
        static void RedTitle(string Title, string Content)
        {
            Console.ForegroundColor = ConsoleColor.Red;  // Set console text color to red
            Console.Write(Title);  // Display the title in red color
            Console.ForegroundColor = ConsoleColor.White;  // Set console text color back to white
            Console.Write(Content);  // Display the content in white color
        }
    }

    
    //A log class responsible for adding log functionality. Added as a clase to be used globally through the application
    public class Logs
    {
        protected List<string> AppLogs = new List<string>();  // List to store the application logs
        int LogsCount = 0;  // Counter for the number of logs


        // Method to add a log entry to the list. Showcasesn the user input and showcauses the notation and evaluation
        public void AddLog(string Infix, string Postfix, string Evaluation)
        {
            LogsCount++;  // Increment the logs count
            StringBuilder sm = new StringBuilder();
            sm.AppendFormat("{0}) =>  {1} RPN: {2} | Evaluation: {3}", LogsCount, (!string.IsNullOrEmpty(Infix) ? (" Infix: " + Infix + " | ") : ""), Postfix, Evaluation);
            AppLogs.Add(sm.ToString());  // Add the formatted log entry to the list
        }


        // Method to display all the logs
        public void ShowLogs()
        {
            foreach (var log in AppLogs)
            {
                Console.ForegroundColor = ConsoleColor.Red;  // Set console text color to red
                Console.Write(log.Substring(0, log.IndexOf(">")));  // Display the log count in red color
                Console.ForegroundColor = ConsoleColor.White;  // Set console text color back to white
                Console.WriteLine(log.Substring(log.IndexOf(">") + 2));  // Display the remaining log content in white color
            }
        }
    }
}