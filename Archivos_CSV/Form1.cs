using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Archivos_CSV
{
    public partial class Form1 : Form
    {
        DataTable tablaCSV = new DataTable();
        string rutaArchivoActual = "";
        private object saveFileDialog;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                rutaArchivoActual = openFileDialog.FileName; // <-- AGREGA ESTA LÍNEA
            {
                tablaCSV.Clear();
                tablaCSV.Columns.Clear();
                string[] lineas = System.IO.File.ReadAllLines(openFileDialog.FileName);

                if (lineas.Length > 0)
                {
                    string[] encabezados = lineas[0].Split(',');
                    foreach (string encabezado in encabezados)
                    {
                        tablaCSV.Columns.Add(encabezado);
                    }
                    for (int i = 1; i < lineas.Length; i++)
                    {
                        string[] datos = lineas[i].Split(',');
                        tablaCSV.Rows.Add(datos);
                    }
                    dgvDatos.DataSource = tablaCSV;
                    // --- NUEVO CÓDIGO: Bloquear todas las celdas cargadas ---
                    foreach (DataGridViewRow fila in dgvDatos.Rows)
                    {
                        foreach (DataGridViewCell celda in fila.Cells)
                        {
                            celda.ReadOnly = true;
                        }
                    }
                }
                
            }
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos CSV (*.csv)|*.csv";
            saveFileDialog.Title = "Crear nuevo archivo CSV";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Guardamos la ruta del nuevo archivo
                rutaArchivoActual = saveFileDialog.FileName;

                // Limpiamos la tabla en pantalla
                tablaCSV.Clear();
                tablaCSV.Columns.Clear();

                // Creamos unas columnas por defecto para poder empezar a escribir
                // Puedes cambiar estos nombres por los que necesites
                tablaCSV.Columns.Add("Columna1");
                tablaCSV.Columns.Add("Columna2");
                tablaCSV.Columns.Add("Columna3");

                // Creamos el archivo físico vacío (solo con los encabezados)
                using (StreamWriter sw = new StreamWriter(rutaArchivoActual))
                {
                    sw.WriteLine("Columna1,Columna2,Columna3");
                }
                // 5. ¡EL TRUCO AQUÍ! Refrescamos el DataGridView
                dgvDatos.DataSource = null; // Quitamos lo que había
                dgvDatos.DataSource = tablaCSV; // Ponemos la nueva tabla limpia

                MessageBox.Show("Archivo nuevo creado. Ya puedes empezar a agregar datos en la tabla.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // Verifica que haya una fila seleccionada y que no sea la fila vacía de nuevo ingreso
            if (dgvDatos.CurrentCell != null && !dgvDatos.Rows[dgvDatos.CurrentRow.Index].IsNewRow)
            {
                dgvDatos.Rows.RemoveAt(dgvDatos.CurrentRow.Index);
            }
            else
            {
                MessageBox.Show("Por favor, selecciona una fila válida para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgvDatos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Al momento de que el usuario termina de escribir, bloqueamos esa celda específica
            dgvDatos.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (dgvDatos.CurrentCell != null && !dgvDatos.Rows[dgvDatos.CurrentRow.Index].IsNewRow)
            {
                dgvDatos.CurrentCell.ReadOnly = false;
                dgvDatos.BeginEdit(true);
            }
            else
            {
                MessageBox.Show("Por favor, selecciona una fila válida para modificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {

            // 1. Aseguramos que cualquier edición en la celda se confirme
            dgvDatos.EndEdit();
            tablaCSV.AcceptChanges();

            // 2. Verificamos si realmente hay un archivo abierto
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                MessageBox.Show("No hay ningún archivo abierto para guardar. Primero abre un archivo CSV.",
                                "Aviso",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return; // Salimos del método para no causar un error
            }

            try
            {
                // 3. Sobrescribimos el archivo original directamente
                using (StreamWriter sw = new StreamWriter(rutaArchivoActual))
                {
                    // Escribir los encabezados
                    for (int i = 0; i < tablaCSV.Columns.Count; i++)
                    {
                        sw.Write(tablaCSV.Columns[i].ColumnName);
                        if (i < tablaCSV.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.WriteLine();

                    // Escribir las filas
                    foreach (DataRow fila in tablaCSV.Rows)
                    {
                        for (int i = 0; i < tablaCSV.Columns.Count; i++)
                        {
                            sw.Write(fila[i].ToString());
                            if (i < tablaCSV.Columns.Count - 1)
                            {
                                sw.Write(",");
                            }
                        }
                        sw.WriteLine();
                    }
                }

                MessageBox.Show("Los cambios se han guardado en el archivo original.",
                                "Guardado Rápido Exitoso",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar guardar el archivo:\n\n" + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}
 