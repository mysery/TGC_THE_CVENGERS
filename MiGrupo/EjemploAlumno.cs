using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using AlumnoEjemplos.MiGrupo;
using Examples.Quake3Loader;
using Examples.Shaders;
using TgcViewer.Utils.Shaders;


namespace AlumnoEjemplos.MiGrupo
{
 
    public class Juego : TgcExample
    {
        const float MOVEMENT_SPEED = 400f;
        FPSCustomCamera camera = new FPSCustomCamera();

        List<TgcMesh> meshes;
        
        //Variable para esfera
        TgcBoundingSphere sphere;

        Vector3 lightDirAnterior = (new Vector3(500, 0, 1) - new Vector3(500, 60, 900));
        Vector3 lookAtAnterior = new Vector3(500, 0, 1);




        public override string getCategory()
        {
            return "MiGrupo";
        }

        public override string getName()
        {
            return "EjemploAlumno";
        }

        public override string getDescription()
        {
            return "Orfanato.";
        }


        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Creamos caja de colision
            sphere = new TgcBoundingSphere(new Vector3(160, 60, 240), 20f);

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;


            

            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Orfanato-TgcScene.xml");
            meshes = scene.Meshes;

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("variableX");
            GuiController.Instance.UserVars.addVar("variableY");
            GuiController.Instance.UserVars.addVar("variableZ");


            GuiController.Instance.UserVars.addVar("LookAt");


            GuiController.Instance.UserVars.addVar("Posicion");

            GuiController.Instance.UserVars.addVar("LightPosicion");
            GuiController.Instance.UserVars.addVar("LightDir");

            GuiController.Instance.UserVars.addVar("lukat");


            camera.Enable = true;

            camera.setCamera(new Vector3(500, 60, 900), new Vector3(500, 0, 1));


        }

        bool pepe = true;

        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            sphere.setCenter(camera.getPosition());

            d3dDevice.BeginScene();


            ///////////////////////////////////////////// LUCES  /////////////////////////////////////////////////////////////

            Microsoft.DirectX.Direct3D.Effect currentShader;
            //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight
            currentShader = GuiController.Instance.Shaders.TgcMeshSpotLightShader;

            //Aplicar a cada mesh el shader actual
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
            }

            //Actualzar posici�n de la luz


            Vector3 lightPos = camera.getPosition();
            GuiController.Instance.UserVars.setValue("LightPosicion", lightPos);

            Vector3 lightDir;

            if (lookAtAnterior == camera.getLookAt())
            {

                lightDir = lightDirAnterior;
            }

            else
            {
                lightDir = (camera.getLookAt() - camera.getPosition());
                lookAtAnterior = camera.getLookAt();
                lightDirAnterior = lightDir;

            }

            GuiController.Instance.UserVars.setValue("lukat", lookAtAnterior);

            lightDir.Normalize();

            GuiController.Instance.UserVars.setValue("LightDir", lightDir);

            foreach (TgcMesh mesh in meshes)
                        {

                            //Cargar variables shader de la luz
                            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
                            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(camera.getPosition()));
                            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                            mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));

                            mesh.Effect.SetValue("lightIntensity", (float)60f);
                            mesh.Effect.SetValue("lightAttenuation", (float)0.8f);
                            mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad((float)39f));
                            mesh.Effect.SetValue("spotLightExponent", (float)7f);

                                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                                mesh.Effect.SetValue("materialSpecularExp", (float)200f);


                            //Renderizar modelo
                            mesh.render();
                        }

           

            ///////////////////////////////////////////// LUCES  /////////////////////////////////////////////////////////////




            //Render de cada mesh
            foreach (TgcMesh mesh in meshes)
            {

                mesh.render();
            }


            d3dDevice.EndScene();

            //Guardar posicion original antes de cambiarla
            Vector3 originalPos = camera.getPosition();
            Vector3 originalLook = camera.getLookAt();
            Matrix view = camera.ViewMatrix;
            Vector3 z = camera.ZAxis;
            Vector3 x = camera.XAxis;
            Vector3 y = camera.YAxis;
            Vector3 direction = camera.Direction;

            

        //    Vector3 velocity = camera.CurrentVelocity;

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("variableX", direction.X);
            GuiController.Instance.UserVars.setValue("variableY", direction.Y);
            GuiController.Instance.UserVars.setValue("variableZ", direction.Z);

            GuiController.Instance.UserVars.setValue("LookAt", camera.getLookAt());
            GuiController.Instance.UserVars.setValue("Posicion", camera.getPosition());

            //Chequear si el objeto principal en su nueva posici�n choca con alguno de los objetos de la escena.
            //Si es as�, entonces volvemos a la posici�n original.
            //Cada TgcMesh tiene un objeto llamado BoundingBox. El BoundingBox es una caja 3D que representa al objeto
            //de forma simplificada (sin tener en cuenta toda la complejidad interna del modelo).
            //Este BoundingBox se utiliza para chequear si dos objetos colisionan entre s�.
            //El framework posee la clase TgcCollisionUtils con muchos algoritmos de colisi�n de distintos tipos de objetos.
            //Por ejemplo chequear si dos cajas colisionan entre s�, o dos esferas, o esfera con caja, etc.
            bool collisionFound = false;
            foreach (TgcMesh mesh in meshes)
            {
                //Los dos BoundingBox que vamos a testear
                TgcBoundingSphere mainMeshBoundingBox = sphere;
                TgcBoundingBox sceneMeshBoundingBox = mesh.BoundingBox;


                //Hubo colisi�n con un objeto. Guardar resultado y abortar loop.
                if (TgcCollisionUtils.testSphereAABB(mainMeshBoundingBox, sceneMeshBoundingBox))
                {
                    collisionFound = true;
                    break;
                }
            }

            //Si hubo alguna colisi�n, entonces restaurar la posici�n original del mesh
            if (collisionFound)
            {

                
               // camera.ViewMatrix = view;
                camera.setearCamara(originalPos, originalLook, view,x,y,z, direction);
                

            }








        }

        public override void close()
        {
            
            foreach (TgcMesh mesh in meshes)
            {
                mesh.dispose();
            }

            sphere.dispose();
        }

    }
}
