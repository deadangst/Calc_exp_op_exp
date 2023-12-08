using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using Newtonsoft.Json;

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

            // Agrega un manejador para el evento KeyDown del TextBox
            InputBox.KeyDown += InputBox_KeyDown;
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
                        items[^1].Expresion2 = ultimoInput;
                    }
                    else
                    {
                        items.Add(new CalculadoraItem { Expresion = ultimoInput });
                    }
                    InputBox.Clear();
                }
                else
                {
                    if (items.Count > 0)
                    {
                        var ultimoItem = items[^1];
                        ultimoItem.Comentario = "Error léxico, se esperaba un dígito";
                        DataGridCalculadora.Items.Refresh();

                        // Agregar una nueva fila con valores de la fila anterior, excepto el comentario
                        items.Add(new CalculadoraItem
                        {
                            Expresion = ultimoItem.Expresion,
                            Operacion = ultimoItem.Operacion
                        });
                    }
                    else
                    {
                        items.Add(new CalculadoraItem { Comentario = "Error léxico, se esperaba un dígito" });
                    }
                }
            }
        }


        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Ingresar_Click(sender, e);
            }
        }


        private void BtnOperacion_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            operacionActual = btn.Content.ToString();

            if (items.Count > 0)
            {
                var ultimoItem = items[^1];

                if (!string.IsNullOrEmpty(ultimoItem.Expresion) && string.IsNullOrEmpty(ultimoItem.Operacion))
                {
                    if (double.TryParse(ultimoItem.Expresion, out _))
                    {
                        ultimoItem.Operacion = operacionActual;
                        DataGridCalculadora.Items.Refresh();
                    }
                    else
                    {
                        ultimoItem.Comentario = "Error léxico, se esperaba un valor numérico";
                        DataGridCalculadora.Items.Refresh();

                        // Agregar una nueva fila con valores de la fila anterior, excepto el comentario
                        items.Add(new CalculadoraItem
                        {
                            Expresion = ultimoItem.Expresion,
                            Operacion = ultimoItem.Operacion
                        });
                    }
                }
                else
                {
                    ultimoItem.Comentario = "Error léxico, se esperaba un valor numérico";
                    DataGridCalculadora.Items.Refresh();

                    // Agregar una nueva fila con valores de la fila anterior, excepto el comentario
                    items.Add(new CalculadoraItem
                    {
                        Expresion = ultimoItem.Expresion,
                        Operacion = ultimoItem.Operacion
                    });
                }
            }
            else
            {
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            GuardarDatos(); // Guarda los datos al cerrar la ventana
        }

        private void GuardarDatos()
        {
            string directorio = @"C:\Calculadora_Proyecto";
            string archivo = Path.Combine(directorio, "Resultados_Calc.json");

            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            string jsonData = JsonConvert.SerializeObject(items);
            File.WriteAllText(archivo, jsonData);
        }


        private void CargarDatos()
        {
            string archivo = @"C:\Calculadora_Proyecto\Resultados_Calc.json";

            if (File.Exists(archivo))
            {
                string jsonData = File.ReadAllText(archivo);
                var loadedItems = JsonConvert.DeserializeObject<ObservableCollection<CalculadoraItem>>(jsonData);
                foreach (var item in loadedItems)
                {
                    items.Add(item);
                }
            }
        }
        public class CalculadoraItem
        {
            public string Expresion { get; set; }
            public string Operacion { get; set; }
            public string Expresion2 { get; set; }
            public string Comentario { get; set; }
        }
    }
}
