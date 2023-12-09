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
    // Clase principal que representa la ventana de la calculadora.
    public partial class MainWindow : Window
    {
        // Colección que almacena los elementos de la calculadora
        private ObservableCollection<CalculadoraItem> items = new ObservableCollection<CalculadoraItem>();
        private string operacionActual = string.Empty;
        private string ultimoInput = string.Empty;

        // Constructor de la ventana principal.
        public MainWindow()
        {
            InitializeComponent();
            DataGridCalculadora.ItemsSource = items;
            CargarDatos();

            // Agrega un manejador para el evento KeyDown del TextBox
            InputBox.KeyDown += InputBox_KeyDown;
        }

        // Manejador del evento Click para el botón "Ingresar".
        // Procesa la entrada del usuario y actualiza la interfaz gráfica según corresponda.
        private void Ingresar_Click(object sender, RoutedEventArgs e)
        {
            ultimoInput = InputBox.Text;
            if (!string.IsNullOrWhiteSpace(ultimoInput))
            {
                if (double.TryParse(ultimoInput, out _))
                {
                    if (items.Count > 0 && !string.IsNullOrEmpty(items[^1].Operacion))
                    {
                        if (string.IsNullOrEmpty(items[^1].Expresion2))
                        {
                            // Si ya hay una operación pero Expresion2 está vacío, se coloca el número en Expresion2
                            items[^1].Expresion2 = ultimoInput;
                            DataGridCalculadora.Items.Refresh(); // Actualiza el DataGrid para mostrar el cambio

                            // Deshabilitar el botón "Borrar Operación"
                            BtnBorrarOperacion.IsEnabled = false;
                        }
                        else
                        {
                            // Si ya hay un número en Expresion2, se crea una nueva fila
                            items.Add(new CalculadoraItem { Expresion = ultimoInput });
                        }
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

        // Manejador del evento KeyDown para la caja de texto.
        // Permite al usuario presionar Enter para ingresar datos.
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Ingresar_Click(sender, e);
            }
        }

        // Manejador del evento Click para los botones de operaciones matemáticas.
        // Agrega la operación seleccionada a la fila activa en el DataGrid.
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

                        // Habilitar el botón "Borrar Operación"
                        BtnBorrarOperacion.IsEnabled = true;
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


        // Manejador del evento Click para el botón "Borrar Operación".
        // Permite al usuario borrar la operación matemática seleccionada previamente.
        private void BtnBorrarOperacion_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count > 0)
            {
                items[^1].Operacion = string.Empty;
                BtnBorrarOperacion.IsEnabled = false; // Deshabilitar el botón después de borrar la operación
                DataGridCalculadora.Items.Refresh();
            }
        }

        // Manejador del evento Click para el botón "Borrar Datos".
        // Limpia todos los datos del DataGrid y reinicia la interfaz de la calculadora.
        private void BtnBorrarDatos_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count > 0)
            {
                //items[^1].Expresion = string.Empty;
                //items[^1].Expresion2 = string.Empty;
                items.Clear(); // Limpia toda la colección
                DataGridCalculadora.Items.Refresh();
            }
        }

        // Manejador del evento Click para el botón "Resultado".
        // Calcula el resultado de la operación actual y lo muestra en la interfaz.
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

        // Evento que se activa al cerrar la ventana. Guarda los datos actuales en un archivo JSON.
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            GuardarDatos(); // Guarda los datos al cerrar la ventana
        }

        // Guarda los datos de la calculadora en un archivo JSON.
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

        // Carga los datos de la calculadora desde un archivo JSON.
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

        // Clase que representa un elemento de la calculadora
        public class CalculadoraItem
        {
            public string Expresion { get; set; }
            public string Operacion { get; set; }
            public string Expresion2 { get; set; }
            public string Comentario { get; set; }
        }
    }
}
