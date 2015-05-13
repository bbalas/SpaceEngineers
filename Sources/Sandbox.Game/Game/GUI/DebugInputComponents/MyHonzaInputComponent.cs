﻿#region Using

using Havok;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System.Collections.Generic;
using VRage.Audio;
using VRage.Input;
using VRageMath;

#endregion

namespace Sandbox.Game.Gui
{
    class MyHonzaInputComponent : MyDebugComponent
    {
        public static float MassMultiplier = 100;

        private static float m_steer = 0f;
        private static float m_damping = 0.01f;
        private static float m_strength = 0.002f;
        private static float m_max = 0.2f;
        private static float m_min;
        private static long m_lastMemory;
        private static HkMemorySnapshot? m_snapA;

        public static bool ApplyMassMultiplier;

        public enum ShownMassEnum
        {
            Havok = 0,
            Real = 1,
            SI = 2,
            None = 3,
            MaxVal
        }
        public static ShownMassEnum ShowRealBlockMass = ShownMassEnum.None;
        private int m_memoryB;
        private int m_memoryA;
        private static bool HammerForce;
        private float RADIUS = 0.005f;

        public override string GetName()
        {
            return "Honza";
        }

        public MyHonzaInputComponent()
        {
            AddShortcut(MyKeys.None, false, false, false, false,
                () => "Hammer (CTRL + Mouse left)",
                null
             );

            AddShortcut(MyKeys.H, true, true, true, false,
                () => "Hammer force: " + (MyHonzaInputComponent.HammerForce ? "ON" : "OFF"),
                delegate
                {
                    MyHonzaInputComponent.HammerForce = !MyHonzaInputComponent.HammerForce;
                    return true;
                }
                );

            AddShortcut(MyKeys.OemPlus, true, true, false, false,
             () => "Radius+: " + RADIUS,
             delegate
             {
                  RADIUS += 0.5f;
                 return true;
             }
             );

             AddShortcut(MyKeys.OemMinus, true, true, false, false,
             () => "",
             delegate
             {
                  RADIUS -= 0.5f;
                 return true;
             }
             );

            AddShortcut(MyKeys.NumPad7, true, false, false, false,
                () => "Shown mass: " + ShowRealBlockMass.ToString(),
                delegate
                {
                    ShowRealBlockMass++;
                    ShowRealBlockMass = (ShownMassEnum)((int)ShowRealBlockMass % (int)ShownMassEnum.MaxVal);
                    return true;
                });
            AddShortcut(MyKeys.NumPad8, true, false, false, false,
                () => "MemA: " + m_memoryA + " MemB: " + m_memoryB + " Diff:" + (m_memoryB - m_memoryA),
               Diff);
        }

        private bool Diff()
        {
            if (!m_snapA.HasValue)
                m_snapA = HkBaseSystem.GetMemorySnapshot();
            else
            {
                var snapB = HkBaseSystem.GetMemorySnapshot();
                int a, b;
                HkMemorySnapshot.Diff(m_snapA.Value, snapB, out a, out b);
                //HkMemorySnapshot.Diff(m_snapA.Value, snapB);
                m_snapA.Value.RemoveReference();
                m_snapA = null;
                snapB.RemoveReference();
                m_memoryA = a;
                m_memoryB = b;
            }
            return true;
        }

        public override bool HandleInput()
        {
            if (base.HandleInput())
                return true;

            bool handled = false;

            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewLeftMouseReleased())
            {
                Hammer();
            }

            if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad1))
            {
                ApplyMassMultiplier = !ApplyMassMultiplier;
                handled = true;
            }
            var mul = 1;
            if (MyInput.Static.IsKeyPress(MyKeys.N))
                mul = 10;
            if (MyInput.Static.IsKeyPress(MyKeys.B))
                mul = 100;
            if (MyInput.Static.IsNewKeyPressed(MyKeys.OemQuotes))
            {
                if (MassMultiplier > 1)
                    MassMultiplier += mul;
                else
                    MassMultiplier *= mul;
                handled = true;
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.OemSemicolon))
            {
                if (MassMultiplier > 1)
                    MassMultiplier -= mul;
                else
                    MassMultiplier /= mul;
                handled = true;
            }

            if (MySector.MainCamera != null)
            {
                List<MyPhysics.HitInfo> lst = new List<MyPhysics.HitInfo>();
                MyPhysics.CastRay(MySector.MainCamera.Position, MySector.MainCamera.Position + MySector.MainCamera.ForwardVector * 100, lst);
                foreach (var hit in lst)
                {
                    VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(600, 10), hit.HkHitInfo.Body.GetEntity().ToString(), Color.White, 1);
                    break;
                }
            }

            if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad9))
                MyScriptManager.Static.LoadData();

            if(MyAudio.Static != null)
            {
                foreach(var em in MyAudio.Static.Get3DSounds())
                {
                    VRageRender.MyRenderProxy.DebugDrawSphere(em.SourcePosition, 0.1f, Color.Red, 1, false);
                }
            }

            return handled;
        }

        private void Hammer()
        {
            var IntersectionStart = MySector.MainCamera.Position;
            var IntersectionDirection = MySector.MainCamera.ForwardVector;
            LineD line = new LineD(IntersectionStart, IntersectionStart + IntersectionDirection * 200);

            var m_tmpHitList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(line.From, line.To, m_tmpHitList, MyPhysics.ObjectDetectionCollisionLayer);
            // Remove character hits.
            m_tmpHitList.RemoveAll(delegate(MyPhysics.HitInfo hit)
            {
                return (hit.HkHitInfo.Body.GetEntity() == MySession.ControlledEntity.Entity);
            });

            if (m_tmpHitList.Count == 0)
                return;

            MyEntity closestGrid = null;
            MyPhysics.HitInfo closestHit = default(MyPhysics.HitInfo);

            foreach (var hit in m_tmpHitList)
            {
                if (hit.HkHitInfo.Body != null)
                {
                    closestGrid = hit.HkHitInfo.Body.GetEntity() as MyEntity;
                    closestHit = hit;
                    break;
                }
            }
            if (closestGrid == null)
                return;
            HkdFractureImpactDetails details = HkdFractureImpactDetails.Create();
            details.SetBreakingBody(closestGrid.Physics.RigidBody);
            details.SetContactPoint(closestGrid.Physics.WorldToCluster(closestHit.Position));
            details.SetDestructionRadius(RADIUS);
            details.SetBreakingImpulse(Sandbox.MyDestructionConstants.STRENGTH * 10);
            if(HammerForce)
                details.SetParticleVelocity(-line.Direction * 20);
            details.SetParticlePosition(closestGrid.Physics.WorldToCluster(closestHit.Position));
            details.SetParticleMass(1000000);
            //details.ZeroColidingParticleVelocity();
            details.Flag = details.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_RECURSE;
            if (closestGrid.Physics.HavokWorld.DestructionWorld != null)
            {
                MyPhysics.FractureImpactDetails destruction = new MyPhysics.FractureImpactDetails();
                destruction.Details = details;
                destruction.World = closestGrid.Physics.HavokWorld;
                MyPhysics.EnqueueDestruction(destruction);
                //closestGrid.Physics.HavokWorld.DestructionWorld.TriggerDestruction(ref details);
            }
            //details.RemoveReference();
        }

        private static bool SpawnBreakable(bool handled)
        {
            //if (MyInput.Static.IsNewKeyPressed(Keys.NumPad2))
            //{
            //    Vector3 he = new Vector3(1, 1, 1);
            //    HkShape psh = new HkBoxShape(he);
            //    HkMassProperties mp = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(he, 5);
            //    HkRigidBodyCinfo rbInfo = new HkRigidBodyCinfo();
            //    rbInfo.Shape = psh;
            //    rbInfo.SetMassProperties(mp);
            //    rbInfo.MotionType = HkMotionType.Dynamic;
            //    rbInfo.QualityType = HkCollidableQualityType.Moving;
            //    HkRigidBody rb = new HkRigidBody(rbInfo);
            //    HkdBreakableShape sh = new HkdBreakableShape(psh, ref mp);

            //    //sh.Fracture(MyPhysics.HavokWorld);
            //    //sh.Fracture(MyPhysics.DestructionWorld);
            //    sh.SetMassRecursively(100);
            //    sh.SetStrenghtRecursively(MyHonzaInputComponent.STRENGHT, 0.5f);

            //    HkdShapeInstanceInfo si = new HkdShapeInstanceInfo(sh, null, null);
            //    HkdCreateBodyInput ci = new HkdCreateBodyInput();

            //    //HkBreakableBody(HkBreakableShape^ breakableShape, HkRigidBody^ body)
            //    HkdBreakableBody body = new HkdBreakableBody(sh, rb);
            //    MyPhysics.DestructionWorld.AddBreakableBody(body);
            //    psh.RemoveReference();
            //    handled = true;
            //}

            //if (MyInput.Static.IsNewKeyPressed(Keys.NumPad3))
            //{
            //    var factory = MyPhysics.DestructionWorld.GetBreakableBodyFactory();
            //    Vector3 halfExtents = new Vector3(1, 1, 1);
            //    HkShape shape = new HkBoxShape(halfExtents);
            //    HkMassProperties massProp = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(halfExtents, 5);

            //    int shapeCount = 10;
            //    List<HkdShapeInstanceInfo> shapeInfos = new List<HkdShapeInstanceInfo>(shapeCount);
            //    for (int i = 0; i < shapeCount; i++)
            //    {
            //        HkdBreakableShape breakableShape = new HkdBreakableShape(shape, ref massProp);
            //        shapeInfos.Add(new HkdShapeInstanceInfo(breakableShape, null, new Vector3(0, 2 * i, 0)));
            //        var x = breakableShape.ReferenceCount;
            //    }

            //    HkdCompoundBreakableShape compBreakable = new HkdCompoundBreakableShape(null, shapeInfos);
            //    compBreakable.AutoConnect(MyPhysics.HavokWorld);
            //    //for (int i =0 ; i < shapeInfos.Count-1; i++)	
            //    //{
            //    //    Connection connection = Connection.Create();

            //    //    connection.SetShapesFromInfos(shapeInfos[i], shapeInfos[i+1]);
            //    //    connection.PivotA = connection.PivotB = Vector3.Zero;
            //    //    connection.SeparatingNormal = new Vector3(0,1,0);
            //    //    connection.ContactArea = 1.0f;
            //    //    connection.AddToCommonParent();
            //    //}
            //    compBreakable.SetStrenghtRecursively(300, 0.8f);
            //    compBreakable.BuildMassProperties(ref massProp);
            //    HkRigidBodyCinfo rbInfo = new HkRigidBodyCinfo();
            //    rbInfo.Shape = compBreakable.GetShape();
            //    rbInfo.SetMassProperties(massProp);
            //    rbInfo.MotionType = HkMotionType.Dynamic;
            //    rbInfo.QualityType = HkCollidableQualityType.Moving;
            //    HkRigidBody rb = new HkRigidBody(rbInfo);
            //    //CreateBodyInput ci = new CreateBodyInput();
            //    //HkBreakableBody body = fac.CreateBreakableBody(si,ci);
            //    //HkBreakableBody(HkBreakableShape^ breakableShape, HkRigidBody^ body)
            //    HkdBreakableBody body = new HkdBreakableBody(compBreakable, rb);
            //    MyPhysics.DestructionWorld.AddBreakableBody(body);
            //    shape.RemoveReference();
            //    handled = true;
            //}
            return handled;
        }

        public override void Draw()
        {
            base.Draw();
            //if (!MySandboxGame.Static.IsRunning)
            //    return;
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 0.1f), "Destruction: " + (MyPerGameSettings.Destruction ? "ON" : "OFF"), Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(200f, 5f), "(Ctrl+Shift+D)", Color.Red, 0.65f);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.3f, 20f), "Strenght: " + STRENGHT, Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 40f), "Fracture: " + (FRACTURE ? "ON" : "OFF"), Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(160f, 45f), "(Ctrl+Shift+F)", Color.Red, 0.65f);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 60f), "Integrity: " + (MyFakes.DESTRUCTION_STRUCTURAL_INTEGRITY ? "ON" : "OFF"), Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(160f, 65f), "(Ctrl+I)", Color.Red, 0.65f);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 80f), "Position: " + POSITION, Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 100f), "Split: " + (GRID_SPLIT ? "ON" : "OFF"), Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 120f), "AConn: " + (!ManualConnect ? "ON" : "OFF"), Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 140f), "Hammer: " + (DestructionTool ? "ON" : "OFF"), Color.Red, 1);
            
            
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, 0.1f), "MassMultiplier " + MassMultiplier, Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.3f, 20f), ApplyMassMultiplier ? "ON" : "OFF", Color.Red, 1);
            //VRageRender.MyRenderProxy.DebugDrawText2D(new Vector2(0.3f, 40f), "Num1, N, B, ;, '", Color.Red, 1);
        }
    }
}
