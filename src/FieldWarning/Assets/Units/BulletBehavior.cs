/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */

using AssemblyCSharp;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public Bullet bullet; //contains attributes for the shell 
    [Header("Explosion you want to appear when shell hits the target or ground")]
    public GameObject ExplosionPrefab;
    [Header("Trail emitter of this shell prefab - to be disabled on hit")]
    public ParticleSystem TrailEmitter;

    private Rigidbody rigid;

    private float LaunchAngle;
    private Transform StartingTransform;
    private Vector3 TargetCoordinates;
    //class used in the weapon behaviour to set stats of the shell
    public void SetUp(Transform position, Vector3 target, float launchAngel)
    {
        StartingTransform = position;
        TargetCoordinates = target;
        LaunchAngle = launchAngel;
    }

    void Start()
    {
        bullet = new Bullet();
        rigid = GetComponent<Rigidbody>();

        rigid.useGravity = false;
        rigid.isKinematic = true; // means that rigidbody is moved by script and does not affected by physics engine

        Launch();
    }

    private float Gravity = 9.8F;
    private float ForwardSpeed = 0F;
    private float VerticalSpeed = 0F;
    public void Launch()
    {
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 targetXZPos = new Vector3(TargetCoordinates.x, 0.0f, TargetCoordinates.z);

        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        // formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = 9.8F;     //TODO Readjust for our scale usinga utility class
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = TargetCoordinates.y - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        //float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));

        float Vz = 20F;

        float DistanceToHighestPoint = R / 2;
        float TimeToHighestPoint = DistanceToHighestPoint / Vz;
        float GravityEffectToHighestPoint = Gravity * TimeToHighestPoint;

        float Vy = GravityEffectToHighestPoint;

        ForwardSpeed = Vz;
        VerticalSpeed = Vy;

        //Debug.LogFormat("BulletBehavior.Launch: ForwardSpeed={0}, VerticalSpeed={1}, LaunchAngle={2}, R={3}, tanALpha={4}, H={5}",
        //    ForwardSpeed, VerticalSpeed, LaunchAngle, R, tanAlpha, H);

        // create the velocity vector in local space and get it in global space

        //BulletBehavior.Launch: ForwardSpeed=NaN, VerticalSpeed=NaN, LaunchAngle=60, R=15.60759, tanALpha=1.732051, H=-0.8795097
    }

    bool dead = false;
    float prevDistanceToTarget = 100000F;
    void Update()
    {
        if(dead) {
            return;
        }

        transform.Translate(ForwardSpeed * Vector3.forward * Time.deltaTime + VerticalSpeed * Vector3.up * Time.deltaTime);

        VerticalSpeed = VerticalSpeed - (Gravity * Time.deltaTime);


        // small trick to detect if shell is reached the target
        float distanceToTarget = Vector3.Distance(transform.position, TargetCoordinates);
        if(distanceToTarget > prevDistanceToTarget) {
            Explode();
        }
        prevDistanceToTarget = distanceToTarget;
    }
   
    void OnTriggerEnter(Collider other) 
    {
        Explode();
    }

    void Explode() {
        dead = true;
        if(ExplosionPrefab != null) {
            // instantiate explosion
            GameObject explosion = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
            // destroy it in 10 seconds to not trash the scene
            Destroy(explosion, 10F);
        }

        if(TrailEmitter != null) {
            var emission = TrailEmitter.emission;
            emission.enabled = false;
        }

        Destroy(gameObject, 10F);
    }



   

    //public void setBullet(Vector3 StartPosition, Vector3 EndPosition, float Vellocity = 30, int arc = 60)
    //{
    //    bullet._startPosition = StartPosition;
    //    bullet._endPosition = EndPosition;
    //    bullet._vellocity = Vellocity;
    //    bullet._arc = 60;
    //}



}
