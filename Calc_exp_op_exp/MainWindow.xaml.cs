using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<CalculadoraItem> items = new ObservableCollection<CalculadoraItem>();
        private string operacionActual = string.Empty;
        private string ultimoInput = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            DataGridCalculadora.ItemsSource = items;
            CargarDatos();
        }

        private void Ingresar_Click(object sender, RoutedEventArgs e)
        {
            ultimoInput = InputBox.Text;
            if (!string.IsNullOrWhiteSpace(ultimoInput))
            {
                if (double.TryParse(ultimoInput, out _))
                {
                    if (items.Count > 0 && !string.IsNullOrEmpty(items[^1].Operacion) && string.IsNullOrEmpty(items[^1].Expresion2))
                    {
                        // Si ya hay una operación, se coloca el número en la segunda columna de expresión
                        items[^1].Expresion2 = ultimoInput;
                    }
                    else
                    {
                        // Si no hay operación, se coloca el número en la primera columna de expresión
                        items.Add(new CalculadoraItem { Expresion = ultimoInput });
                    }
                    InputBox.Clear();
                }
                else
                {
                    items.Add(new CalculadoraItem { Comentario = "Error léxico, se esperaba un dígito" });
                }
            }
        }



        private void BtnOperacion_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            operacionActual = btn.Content.ToString();

            if (items.Count > 0)
            {
                var ultimoItem = items[^1];

                // Comprobar si el último item tiene una expresión numérica y aún no tiene una operación
                if (!string.IsNullOrEmpty(ultimoItem.Expresion) && string.IsNullOrEmpty(ultimoItem.Operacion))
                {
                    // Si el último item es un número, agregar la operación
                    if (double.TryParse(ultimoItem.Expresion, out _))
                    {
                        ultimoItem.Operacion = operacionActual;
                        DataGridCalculadora.Items.Refresh(); // Actualizar el DataGrid
                    }
                    else
                    {
                        // Si el último item no es un número, mostrar error
                        items.Add(new CalculadoraItem { Comentario = "Error léxico, se esperaba un valor numérico" });
                    }
                }
                else
                {
                    // Si el último item ya tiene una operación o no es un número, mostrar error
                    items.Add(new CalculadoraItem { Comentario = "Error léxico, se esperaba un valor numérico" });
                }
            }
            else
            {
                // Si no hay items, mostrar error
                items.Add(new CalculadoraItem { Comentario = "Error léxico, se esperaba un valor numérico" });
            }
        }


        private void BtnBorrarOperacion_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count > 0)
            {
                items[^1].Operacion = string.Empty;
            }
        }

        private void BtnBorrarDatos_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count > 0)
            {
                items[^1].Expresion = string.Empty;
                items[^1].Expresion2 = string.Empty;
            }
        }

        private void BtnResultado_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count > 0)
            {
                var ultimoItem = items[^1];
                if (!string.IsNullOrEmpty(ultimoItem.Operacion) &&
                    double.TryParse(ultimoItem.Expresion, out double valor1) &&
                    double.TryParse(ultimoItem.Expresion2, out double valor2))
                {
                    double resultado = 0;
                    switch (ultimoItem.Operacion)
                    {
                        case "+":
                            resultado = valor1 + valor2;
                            break;
                        case "-":
                            resultado = valor1 - valor2;
                            break;
                        case "*":
                            resultado = valor1 * valor2;
                            break;
                        case "/":
                            resultado = valor1 / valor2;
                            break;
                    }

                    items.Add(new CalculadoraItem { Expresion = resultado.ToString() });
                }
            }
        }


        private void CargarDatos()
        {
            string archivo = @"C:\Calculadora_Proyecto\Resultados_Calc.txt";

            if (File.Exists(archivo))
            {
                using (StreamReader file = new StreamReader(archivo))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 4)
                        {
                            items.Add(new CalculadoraItem
                            {
                                Expresion = parts[0],
                                Operacion = parts[1],
                                Expresion2 = parts[2],
                                Comentario = parts[3]
                            });
                        }
                    }
                }
            }
        }

        private void GuardarDatos()
        {
            string directorio = @"C:\Calculadora_Proyecto";
            string archivo = Path.Combine(directorio, "Resultados_Calc.txt");

            // Crear directorio si no existe
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            using (StreamWriter file = new StreamWriter(archivo, false))
            {
                foreach (var item in items)
                {
                    file.WriteLine($"{item.Expresion}|{item.Operacion}|{item.Expresion2}|{item.Comentario}");
                }
            }
        }

        // Más lógica según sea necesario
    }




    public class CalculadoraItem
    {
        public string Expresion { get; set; }
        public string Operacion { get; set; }
        public string Expresion2 { get; set; }
        public string Comentario { get; set; }
    }
}
