﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Artngame.INfiniDy;

namespace SimpleGrass
{

    public class InteractCollider : MonoBehaviour
    {

        public SimpleGrassChunk ChunkHandler;
       // public GameObject Chips;
        // Use this for initialization
        void Start()
        {

        }
        //public AudioClip Chop;
        //public AudioClip Bash;
        //public AudioSource Basher;

       // public float Remove_after = 3;

        //List<GameObject> To_erase = new List<GameObject>();
        //List<float> To_erase_timing = new List<float>();

        void OnTriggerStay(Collider other)
        {

            //if (TreeHandler != null && TreeHandler.AddChopHandler)
            //{
            //    TreeHandler.Health -= 5111;

            //    if (Chop != null)
            //    {
            //        if (Basher != null)
            //        {
            //            Basher.clip = Chop;
            //            Basher.PlayOneShot(Chop);
            //        }
            //    }

            //    if (Chips != null)
            //    {
            //        GameObject AA = (GameObject)Instantiate(Chips, other.gameObject.transform.position, other.gameObject.transform.rotation);
            //        AA.SetActive(true);

            //        AA.name = "Chips";
            //        AA.transform.parent = this.gameObject.transform;

            //        To_erase.Add(AA);
            //        To_erase_timing.Add(Time.fixedTime);
            //    }
            //}
        }

       // float timer;

        // Update is called once per frame
        //void Update()
        //{
            //for (int i = To_erase.Count - 1; i >= 0; i--)
            //{
            //    if (Time.fixedTime - To_erase_timing[i] > Remove_after)
            //    {
            //        To_erase.RemoveAt(i);
            //        To_erase_timing.RemoveAt(i);
            //    }
            //}
        //}
    }

}