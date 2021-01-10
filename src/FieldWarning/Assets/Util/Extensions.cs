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

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
public static class Extensions
{
    /*public static override Vector3 getFavorite(this Matchable<Vector3> match, List<Vector3> matchees)
    {
        return new Vector3();
    }*/
    public static void Match<T>(this List<Matchable<T>> matchers, List<T> matchees)
    {
        var matches = new Dictionary<T, MatchStruct<T>>();

        while (matchers.Count > 0) {
            var matcher = matchers[0];

            matchees.Sort((x, y) => matcher.Compare(x, y));

            for (int i = 0; i < matchees.Count; i++) {
                var score = matcher.GetScore(matchees[i]);

                if (!matches.ContainsKey(matchees[i])) {
                    matches.Add(matchees[i], new MatchStruct<T>());
                }

                if (matches[matchees[i]].score > score) {
                    matchers.RemoveAt(0);
                    if (matches[matchees[i]].indivdual != null) 
                        matchers.Add(matches[matchees[i]].indivdual);
                    matches[matchees[i]] = new MatchStruct<T>(matcher, score);
                    break;
                }
            }
        }

        foreach (var p in matchees) {
            if (!matches.ContainsKey(p)) 
                return;
            matches[p].indivdual.SetMatch(p);

        }
        /*while (matchers.Count > 0 && matchees.Count > 0)
        {
            float score = Single.NegativeInfinity;
            int scoreIndex = 0;
            for (int i = 0; i < matchers.Count;i++ )
            {
                var m = matchers[i].getFavorite(matchees);
                var newScore = matchers[i].getScore(m);
                if (newScore > score)
                {
                    score = newScore;
                    scoreIndex = i;
                }  
            }
            var match = matchers[scoreIndex];
            var matchee = match.getFavorite(matchees);
            match.setMatch(matchee);
            matchers.RemoveAt(scoreIndex);
            matchees.Remove(matchee);
        }*/
    }
    private class MatchStruct<T>
    {
        public float score;
        public Matchable<T> indivdual;
        public MatchStruct(Matchable<T> i = null, float s = Single.PositiveInfinity)
        {
            score = s;
            indivdual = i;
        }
    }
    public static int Compare<T>(this Matchable<T> m, T candidate1, T candidate2)
    {
        if (candidate1 == null) {
            return 1;
        }
        if (candidate2 == null) {
            return -1;
        }
        var score1 = m.GetScore(candidate1);
        var score2 = m.GetScore(candidate2);
        if (score1 < score2) {
            return -1;
        } else if (score1 > score2) {
            return 1;
        } else {
            return 0;
        }
    }
    public static T getFavorite<T>(this Matchable<T> m, List<T> matchees)
    {

        T favorite = matchees.First();
        var bestScore = Single.PositiveInfinity;
        foreach (var candidate in matchees) {
            var score = m.GetScore(candidate);
            if (score < bestScore) {
                favorite = candidate;
                bestScore = score;
            }
        }
        return favorite;
    }
    public static void ApplyMaterialRecursively(this GameObject obj, Material mat)
    {
        foreach (Renderer renderer in obj.GetComponents<Renderer>())
        {
            renderer.material = mat;
        }
        for (int i = 0; i < obj.transform.childCount; i++) 
        {
            obj.transform.GetChild(i).gameObject.ApplyMaterialRecursively(mat);
        }
    }
    public static void BlackenRecursively(this GameObject obj)
    {
        foreach (Renderer renderer in obj.GetComponents<Renderer>())
        {
            Material mat = Resources.Load<Material>("Wreck");
            renderer.material = mat;
        }
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.BlackenRecursively();
        }
    }
    public static float unwrapDegree(this float f)
    {
        while (f > 180) f -= 360;
        while (f < -180) f += 360;
        return f;
    }
    public static float unwrapRadian(this float f)
    {
        while (f > Mathf.PI) f -= 2 * Mathf.PI;
        while (f < -Mathf.PI) f += 2 * Mathf.PI;
        return f;
    }
    public static float getDegreeAngle(this Vector3 v)
    {
        return Mathf.Rad2Deg * getRadianAngle(v);
    }
    public static float getRadianAngle(this Vector3 v)
    {
        return Mathf.Atan2(v.z, v.x);
    }
    public static Vector3 getCenterOfMass(this List<MonoBehaviour> list)
    {
        return list.ConvertAll(x => x.gameObject).getCenterOfMass();
    }
    public static Vector3 getCenterOfMass(this List<GameObject> list)
    {
        Vector3 com = new Vector3();
        list.ForEach(x => com += x.transform.position);
        return com / list.Count;

    }

    public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
    {
        List<GameObject> taggedGameObjects = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++) {
            Transform child = parent.GetChild(i);
            if (child.tag == tag) {
                taggedGameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0) {
                taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
            }
        }
        return taggedGameObjects;
    }

    public static float NextFloat(this System.Random random, double minimum, double maximum)
    {
        return (float)(random.NextDouble() * (maximum - minimum) + minimum);
    }
}
public interface Matchable<T>
{
    void SetMatch(T match);

    float GetScore(T matchees);
}
