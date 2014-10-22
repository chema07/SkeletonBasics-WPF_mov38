﻿
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System;
    using Microsoft.Kinect;

    public partial class MainWindow : Window
    {
        enum MOV_STATE { ERROR, INICIAL, HACIA_DELANTE, HACIA_ORIGEN, DETECTADO };
        private MOV_STATE estado = MOV_STATE.INICIAL; // estado de la ejecución del movimiento
        private SkeletonPoint p_inicial; // punto inicial del movimiento en 3D
        private float distancia = 0.15f; // distancia en metros
        private float error_perc = 0.05f; // porcentaje de error en la postura (d ±e%)
        private float error_measures = 0.025f; // error en las medidas kinect por componente espacial (v.z ±e)
        private float error_intention = 0.05f; // margen a satisfacer para detectar intención en el movimiento

        void movimiento(Skeleton skel)
        {
            float distancia;
            float error_val = this.distancia*this.error_perc;
            SkeletonPoint tmp = skel.Joints[JointType.HipCenter].Position;
            textBox1.Clear();
            textBox1.AppendText("Hip Center: " + tmp.X + ", " + tmp.Y + ", " + tmp.Z + "\n\n");
            textBox1.AppendText("ESTADO: ");
            switch (this.estado)
            {
                case MOV_STATE.DETECTADO:
                    textBox1.AppendText("DETECTADO");
                    break;
                case MOV_STATE.ERROR:
                    textBox1.AppendText("ERROR");
                    break;
                case MOV_STATE.HACIA_DELANTE:
                    textBox1.AppendText("HACIA_DELANTE");
                    break;
                case MOV_STATE.HACIA_ORIGEN:
                    textBox1.AppendText("HACIA_ORIGEN");
                    break;
                case MOV_STATE.INICIAL:
                    textBox1.AppendText("INICIAL");
                    break;
            }
            textBox1.AppendText("\n");

            if (this.estado == MOV_STATE.ERROR) {
                // Esperamos que el usuario regrese al punto inicial para repetir el movimiento.
                // distancia de la cadera al punto inicial
                distancia = (float)Math.Sqrt((tmp.X - this.p_inicial.X) * (tmp.X - this.p_inicial.X) +
                    (tmp.Y - this.p_inicial.Y) * (tmp.Y - this.p_inicial.Y) +
                    (tmp.Z - this.p_inicial.Z) * (tmp.Z - this.p_inicial.Z));
                textBox1.AppendText("Distancia desde pos. inicial: " + distancia + "\n");

                if (distancia <= (error_val + error_measures)) {
                    this.estado = MOV_STATE.HACIA_DELANTE;
                }
            } else if (this.estado == MOV_STATE.INICIAL) {
                // Establecer el punto inicial (final) del movimiento
                p_inicial = skel.Joints[JointType.HipCenter].Position;
                this.estado = MOV_STATE.HACIA_DELANTE;
            } else if (this.estado == MOV_STATE.HACIA_DELANTE) {
                // distancia de la cadera al punto inicial
                distancia = (float) Math.Sqrt((tmp.X - this.p_inicial.X) * (tmp.X - this.p_inicial.X) + 
                    (tmp.Y - this.p_inicial.Y) * (tmp.Y - this.p_inicial.Y) + 
                    (tmp.Z - this.p_inicial.Z) * (tmp.Z - this.p_inicial.Z));
                textBox1.AppendText("Distancia desde pos. inicial: " + distancia + "\n");

                if ((tmp.Z - this.p_inicial.Z) > (2*error_measures + error_val + error_intention)) {
                    // Detecta movimiento "intencionado" en Z+ desde la posición inicial.
                    // Si dos puntos son el mismo, con un error de X cm en la medición por cada componente, 
                    // en el peor caso distan 2*X cm (en esa componente). Añadimos un error adicional para
                    // dar margen a la postura y otro para detectar como intencionado el movimiento.
                    this.estado = MOV_STATE.ERROR;
                } else if ((this.distancia - error_val - error_measures) <= distancia && 
                    distancia <= (this.distancia + error_val + error_measures)) {
                    // primera fase completada: mover cadera hacia delante una distancia
                    this.estado = MOV_STATE.HACIA_ORIGEN;
                }

            } else if (this.estado == MOV_STATE.HACIA_ORIGEN) {
                // distancia de la cadera al punto inicial
                distancia = (float)Math.Sqrt((tmp.X - this.p_inicial.X) * (tmp.X - this.p_inicial.X) +
                    (tmp.Y - this.p_inicial.Y) * (tmp.Y - this.p_inicial.Y) +
                    (tmp.Z - this.p_inicial.Z) * (tmp.Z - this.p_inicial.Z));
                textBox1.AppendText("Distancia desde pos. inicial: " + distancia + "\n");

                if (distancia <= (error_val + error_measures))
                {
                    // segunda fase completada: mover cadera hacia atrás hasta la posición inicial
                    this.estado = MOV_STATE.DETECTADO;
                }
            }
        }
    }
}
