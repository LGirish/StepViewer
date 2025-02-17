using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;

using StepAPIService;
using Path = System.IO.Path;

namespace HelixToolkitSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            viewport.Camera.Position = new Point3D(5, 5, 5);
            viewport.Camera.LookDirection = new Vector3D(-5, -5, -5);
            viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
        }

        private static Point3D FromStepPoint(TessModel.Point p) =>
            new Point3D()
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

        private static Point3D[] FromStepPoints(IEnumerable<TessModel.Point> points) =>
            points.Select(FromStepPoint).ToArray();

        private static uint[] ToTriangleIndices(IEnumerable<TessModel.Triangle> triangles) =>
            triangles.SelectMany(tri => new uint[] { tri.I, tri.J, tri.K }).ToArray();
       
        private MeshGeometry3D MapToMeshGeometry3D(TessModel.Tessellation tess)
        {
            var meshBuilder = new MeshBuilder(false, false);

            var points3D = FromStepPoints(tess.Points);
            var indices = ToTriangleIndices(tess.Triangles);

            // Add triangles to the mesh builder
            for (int i = 0; i < indices.Length; i += 3)
            {
                meshBuilder.AddTriangle(points3D[indices[i]], points3D[indices[i + 1]], points3D[indices[i + 2]]);
            }

            return meshBuilder.ToMesh();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "3D Files (*.step;*.stp)|*.step;*.stp|All files (*.*)|*.*",
                Title = "Open step File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _ = LoadModelAsync(openFileDialog.FileName);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task LoadModelAsync(string fileName)
        {
            if (LocalService.Tessellator is null)
                return;

            var extension = Path.GetExtension(fileName).ToLower();
            Model3D? model = null;

            try
            {
                if (extension == ".step" || extension == ".stp")
                {
                    var response = await LocalService.Tessellator.TessellateModel(fileName);
                    if(response is null)
                        return;

                    var customMesh = MapToMeshGeometry3D(response);
                    MaterialGroup customMaterial = new MaterialGroup();
                    DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(200, 245, 222, 179)));
                    customMaterial.Children.Add(diffuseMaterial);
                    SpecularMaterial specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(200, 250, 250, 250)), 25);
                    customMaterial.Children.Add(specularMaterial);

                    model = new GeometryModel3D(customMesh, customMaterial);
                }

                if (model != null)
                {
                    viewport.Children.Clear();
                    viewport.Children.Add(new DefaultLights());
                    viewport.Children.Add(new ModelVisual3D { Content = model });
                    viewport.ZoomExtents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
