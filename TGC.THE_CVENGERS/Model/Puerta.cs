﻿using Microsoft.DirectX;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TGC.Group.Model
{
    internal class Puerta
    {
        private TgcMesh Mesh;
        private bool open;
        public Vector3 posicion { get; set; }
        private float rotacion;
        private Vector3 escala;
        private Vector3 traslado;
        private float angApertura;
        private float contadorApertura;
        private Matrix rotacionActual;
        private Matrix rotacionFinal;
        private Matrix rotacionOriginal;
        public int contadorVillano { get; set; }
        public bool villanoAbriendoPrimera { get; set; }
        public bool villanoAbriendoSiguientes { get; set; }
        private Vector3 posicionOriginal;

        public bool siendoAbiertaPorVillano = false;

        private Puerta puertaSimulada;

        public Puerta(string MediaDir, Vector3 posicionCentro, float rotacion, Vector3 escalas, Vector3 traslado2, float angApertura2)
        {
            this.open = false;

            TgcSceneLoader loadedrL = new TgcSceneLoader();
            this.Mesh = loadedrL.loadSceneFromFile(MediaDir + "ObjetosMapa\\Puerta\\puerta-TgcScene.xml").Meshes[0];
            this.Mesh.AutoTransformEnable = false;
            this.Mesh.AutoUpdateBoundingBox = false;
            this.posicionOriginal = posicionCentro;
            this.posicion = posicionCentro;
            this.traslado = traslado2;
            this.angApertura = angApertura2;
            this.rotacion = rotacion;
            this.contadorVillano = 0;
            this.contadorApertura = 0;
            this.villanoAbriendoPrimera = false;
            this.villanoAbriendoSiguientes = false;

            this.escala = escalas;
            //aca escalas

            Matrix matrizEscala = Matrix.Scaling(escalas.X, escalas.Y, escalas.Z);
            Matrix matrizPosicion = Matrix.Translation(posicionCentro);

            //   this.Mesh.move(posicionCentro);

            float angleY = FastMath.ToRad(rotacion);
            Matrix matrizRotacion = Matrix.RotationY(angleY);
            this.rotacionActual = matrizRotacion;
            this.rotacionOriginal = matrizRotacion;

            this.Mesh.Transform = matrizRotacion * matrizEscala * matrizPosicion;
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);
        }

        public TgcMesh getMesh()
        {
            return this.Mesh;
        }

        public bool getStatus()
        {
            return this.open;
        }

        public void setStatus(bool status)
        {
            open = status;
        }

        public void abrirPuerta()
        {
            Vector3 nuevaPosicion = this.posicion + this.traslado; // los valores estan calculados como ((Ancho/2) - (Espesor/2))
            Matrix translate = Matrix.Translation(nuevaPosicion);
            //seteo la nueva posicion

            this.posicion = nuevaPosicion;

            this.contadorApertura = this.contadorApertura + angApertura;

            float angleY = FastMath.ToRad(contadorApertura);
            Matrix rotation = Matrix.RotationY(angleY);

            rotation = rotation * rotacionActual;

            //

            //this.Mesh.move(nuevaPosicion);

            Matrix matrizEscala = Matrix.Scaling(this.escala.X, this.escala.Y, this.escala.Z);

            this.Mesh.Transform = rotation * matrizEscala * translate;
            this.Mesh.render();
            this.rotacionFinal = rotation;
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);
        }

        public void cerrarPuerta()
        {
            Vector3 nuevaPosicion = this.posicion - this.traslado;
            Matrix translate = Matrix.Translation(nuevaPosicion);

            this.posicion = nuevaPosicion;

            this.contadorApertura = this.contadorApertura - angApertura;

            float angleY = FastMath.ToRad(contadorApertura);
            Matrix rotation = Matrix.RotationY(angleY);
            rotation = rotation * rotacionActual;
            Matrix matrizEscala = Matrix.Scaling(this.escala.X, this.escala.Y, this.escala.Z);

            this.Mesh.Transform = rotation * matrizEscala * translate;
            this.Mesh.render();
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);
        }

        public void accionarPuerta()
        {
            if (this.open)
            {
                this.cerrarPuerta();
            }
            else this.abrirPuerta();
        }

        public void cambiarStatus()
        {
            if (this.open)
            {
                this.setStatus(false);
                rotacionActual = rotacionOriginal;
            }
            else
            {
                this.setStatus(true);
                rotacionActual = rotacionFinal;
            }

            this.contadorApertura = 0;
        }

        public bool puedeAbrirseSinTrabarse(string mediaDir, TgcBoundingSphere spherePuertas)
        {
            return !TgcCollisionUtils.testSphereAABB(spherePuertas, this.BoundingBoxSimulada(mediaDir));
        }

        private TgcBoundingBox BoundingBoxSimulada(string mediaDir)
        {
            puertaSimulada = new Puerta(mediaDir, this.posicionOriginal, this.rotacion, this.escala, this.traslado, this.angApertura);

            if (!this.getStatus())
            {
                puertaSimulada.simularApertura();
                return puertaSimulada.getMesh().BoundingBox;
            }
            else
            {
                puertaSimulada.simularClausura();
                return puertaSimulada.getMesh().BoundingBox;
            }
        }

        private void simularApertura()
        {
            Vector3 nuevaPosicion = this.posicionOriginal + this.traslado * 100; // los valores estan calculados como ((Ancho/2) - (Espesor/2))
            Matrix translate = Matrix.Translation(nuevaPosicion);
            //seteo la nueva posicion

            this.posicion = nuevaPosicion;

            this.contadorApertura = this.contadorApertura + angApertura * 100;

            float angleY = FastMath.ToRad(contadorApertura);
            Matrix rotation = Matrix.RotationY(angleY);

            rotation = rotation * rotacionActual;

            //

            //this.Mesh.move(nuevaPosicion);

            Matrix matrizEscala = Matrix.Scaling(this.escala.X, this.escala.Y, this.escala.Z);

            this.Mesh.Transform = rotation * matrizEscala * translate;
            this.rotacionFinal = rotation;
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);
        }

        private void simularClausura()
        {
            Vector3 nuevaPosicion = this.posicionOriginal + this.traslado * 100; // los valores estan calculados como ((Ancho/2) - (Espesor/2))
            Matrix translate = Matrix.Translation(nuevaPosicion);
            //seteo la nueva posicion

            this.posicion = nuevaPosicion;

            this.contadorApertura = this.contadorApertura + angApertura * 100;

            float angleY = FastMath.ToRad(contadorApertura);
            Matrix rotation = Matrix.RotationY(angleY);

            rotation = rotation * rotacionActual;

            //

            //this.Mesh.move(nuevaPosicion);

            Matrix matrizEscala = Matrix.Scaling(this.escala.X, this.escala.Y, this.escala.Z);

            this.Mesh.Transform = rotation * matrizEscala * translate;
            this.rotacionFinal = rotation;
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);

            nuevaPosicion = this.posicion - this.traslado * 100;
            translate = Matrix.Translation(nuevaPosicion);

            this.posicion = nuevaPosicion;

            this.contadorApertura = this.contadorApertura - angApertura * 100;

            angleY = FastMath.ToRad(contadorApertura);
            rotation = Matrix.RotationY(angleY);
            rotation = rotation * rotacionActual;
            matrizEscala = Matrix.Scaling(this.escala.X, this.escala.Y, this.escala.Z);

            this.Mesh.Transform = rotation * matrizEscala * translate;
            this.Mesh.BoundingBox.transform(this.Mesh.Transform);
        }
    }
}