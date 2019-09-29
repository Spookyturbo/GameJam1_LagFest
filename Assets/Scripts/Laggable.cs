using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laggable : MonoBehaviour
{
    List<ILaggable> laggableObjects;
    //Key laggable object, tuple<netLag, totalLag> totalLag is for regaining
    List<KeyValuePair<Laggable, Tuple<float, float>>> laggedObjects;
    //MaxCpu + deltaCpu effects lag
    [SerializeField] public float maxCpu = 20;
    //Cpu given/taken by external sources
    [SerializeField]
    public float deltaCpu = 0;
    //currentCpu is just for casting like mana to affect deltaCpu on other Laggable objects
    public float usableCpu = 20;
    float regularFps = 60;
    float timePerRegularFrame = 1 / 60;
    float timeSinceLastUpdate = 0;
    float timeSinceLastFixedUpdate = 0;

    public float LagDeltaTime {
        get {return regularFps / Fps * Time.deltaTime; }
    }

    public float FixedLagDeltaTime {
        get { return regularFps / Fps * Time.fixedDeltaTime; }
    }

    public float TimeScale {
        get {return Fps / regularFps; }
    }

    //Only deltaCpu effects maxFps. To ensure scaling of toughness as game goes on
    //and gather more max cpu, use ratio of deltaCpu to maxCpu for fps
    //(This may limit channeling cpu into selfs power, not sure if I want that)
    public float Fps {
        get { return regularFps + Mathf.Clamp(deltaCpu, -regularFps/3, maxCpu * 0.75f) * 3; }
    }

    public float CurrentCpu {
        get { return maxCpu + deltaCpu; }
    }

    void Start() {
        usableCpu = maxCpu;
        laggedObjects = new List<KeyValuePair<Laggable, Tuple<float, float>>>();
        laggableObjects = new List<ILaggable>();
        //Idk why I can't cast this to an ILaggable array but I can't so...
        Component[] components = GetComponents(typeof(ILaggable));
        foreach(Component com in components) {
            if(com is ILaggable l) {
                l.lag = this;
                laggableObjects.Add(l);
            }
        }
    }

    //Positive lag reduces speed negative increases
    public void Lag(Laggable toLag, float amount) {
        amount = Mathf.Min(toLag.CurrentCpu, amount);
        usableCpu -= Mathf.Abs(amount);
        toLag.deltaCpu -= amount;
        int index = laggedObjects.FindIndex(kv => kv.Key.Equals(toLag));

        //Track objects we are lagging
        if(index != -1) {
            laggedObjects[index] = new KeyValuePair<Laggable, Tuple<float, float>>
                (toLag, new Tuple<float, float>(laggedObjects[index].Value.Item1 + amount, laggedObjects[index].Value.Item2 + Mathf.Abs(amount)));
        }
        else {
            laggedObjects.Add(new KeyValuePair<Laggable, Tuple<float, float>>(toLag, new Tuple<float, float>(amount, Mathf.Abs(amount))));
        }
    }

    public void UnlagAll() {
        foreach(KeyValuePair<Laggable, Tuple<float, float>> lagInfo in laggedObjects) {
            Laggable laggedObject = lagInfo.Key;
            float amount = lagInfo.Value.Item1;
            laggedObject.deltaCpu += amount;
            //Item2 stores total lag used on object
            usableCpu += lagInfo.Value.Item2;
        }
        laggedObjects.Clear();
    }

    public void Update()
    {
        while(timeSinceLastUpdate >= LagDeltaTime) {
            foreach(ILaggable laggable in laggableObjects) {
                laggable.LagUpdate();
            }
            timeSinceLastUpdate -= LagDeltaTime;
        }
        timeSinceLastUpdate += Time.deltaTime;
    }
    
    public void FixedUpdate()
    {
        while(timeSinceLastFixedUpdate >= FixedLagDeltaTime) {
            foreach(ILaggable laggable in laggableObjects) {
                laggable.FixedLagUpdate();
            }
            timeSinceLastFixedUpdate -= FixedLagDeltaTime;
        }
        timeSinceLastFixedUpdate += Time.fixedDeltaTime;
    }

    public void AddLaggableObject(ILaggable o) {
        laggableObjects.Add(o);
        o.lag = this;
    }

    public void RemoveLaggableObject(ILaggable o) {
        laggableObjects.Remove(o);
    }

}
