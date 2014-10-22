
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;

    public partial class MainWindow : Window
    {
        enum MOV_STATE { ERROR, INICIAL, HACIA_DELANTE, DELANTE, HACIA_ORIGEN, DETECTADO};
        private MOV_STATE estado = MOV_STATE.INICIAL; // estado de la ejecución del movimiento
        private SkeletonPoint p_inicial; // punto inicial del movimiento en 3D
        private float distancia2 = 0.01f; // distancia al cuadrado: 10 cm de distancia
        private float error = 0.0025f; // porcentaje de error sobre la distancia al cuadrado: 5 percent.

        void movimiento(Skeleton skel)
        {
            float distancia2;
            SkeletonPoint tmp = skel.Joints[JointType.HipCenter].Position;

            if (this.estado == MOV_STATE.INICIAL) {
                // Establecer el punto inicial (final) del movimiento
                p_inicial = skel.Joints[JointType.HipCenter].Position;
                this.estado = MOV_STATE.HACIA_DELANTE;
            } else if (this.estado == MOV_STATE.HACIA_DELANTE) {
                // distancia de la cadera al punto inicial
                distancia2 = (tmp.X - this.p_inicial.X) * (tmp.X - this.p_inicial.X) + 
                    (tmp.Y - this.p_inicial.Y) * (tmp.Y - this.p_inicial.Y) + 
                    (tmp.Z - this.p_inicial.Z) * (tmp.Z - this.p_inicial.Z);

                if ((distancia2 > 0.0075) && (tmp.X > this.p_inicial.X)) {
                    // detecta una dirección incorrecta (salvando el error de medición de Kinect (5 cm))
                    this.estado = MOV_STATE.ERROR;
                } else if ((1.0 - error) * this.distancia2 <= distancia2 && distancia2 <= (1.0 + error) * this.distancia2) {
                    // primera fase completada: mover cadera hacia delante una distancia
                    this.estado = MOV_STATE.DELANTE;
                }

            } else if (this.estado == MOV_STATE.HACIA_ORIGEN) {
                // distancia de la cadera al punto inicial
                distancia2 = (tmp.X - this.p_inicial.X) * (tmp.X - this.p_inicial.X) +
                    (tmp.Y - this.p_inicial.Y) * (tmp.Y - this.p_inicial.Y) +
                    (tmp.Z - this.p_inicial.Z) * (tmp.Z - this.p_inicial.Z);

                if (distancia2 <= this.distancia2*error/2.0)
                {
                    // segunda fase completada: mover cadera hacia atrás hasta la posición inicial
                    this.estado = MOV_STATE.DETECTADO;
                }
            }
        }
    }
}
