using UnityEngine;
using System.Collections;
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
        Dictionary<T, MatchStruct<T>> matches = new Dictionary<T, MatchStruct<T>>();
        while (matchers.Count > 0 )
        {
            bool breaking = false;
            var matcher = matchers[0];
            matchees.Sort((x, y) => matcher.Compare(x, y));
            
            for (int i = 0; i < matchees.Count; i++)
            {
                breaking = true;
                var score = matcher.getScore(matchees[i]);
                if (!matches.ContainsKey(matchees[i]))
                {
                    matches.Add(matchees[i], new MatchStruct<T>());
                }
                
                if (matches[matchees[i]].score > score)
                {
                    
                    matchers.RemoveAt(0);
                    if(matches[matchees[i]].indivdual!=null)matchers.Add(matches[matchees[i]].indivdual);
                    matches[matchees[i]] = new MatchStruct<T>(matcher,score);
                    breaking = false;
                    break;
                }
                
            }
            if (breaking)
            {
                break;
            }
            
        }

        foreach (var p in matchees)
        {
            if (!matches.ContainsKey(p)) return;
            matches[p].indivdual.setMatch(p);
            
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
        if (candidate1 == null)
        {
            return 1;
        }
        if (candidate2 == null)
        {
            return -1;
        }
        var score1 = m.getScore(candidate1);
        var score2 = m.getScore(candidate2);
        if (score1 < score2)
        {
            return -1;
        }
        else if (score1 > score2)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    public static T getFavorite<T>(this Matchable<T> m, List<T> matchees)
    {

        T favorite = matchees.First();
        var bestScore = Single.PositiveInfinity;
        foreach (var candidate in matchees)
        {
            var score = m.getScore(candidate);
            if (score < bestScore)
            {
                favorite = candidate;
                bestScore = score;
            }
        }
        return favorite;
    }
    public static void ApplyShaderRecursively(this GameObject obj, Shader s)
    {
        foreach (var renderer in obj.GetComponents<Renderer>())
        {
            var mat = new Material(renderer.material);
            mat.shader = s;
            mat.color -= Color.black / 2;
            renderer.material = mat;
        }
        for (var i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.ApplyShaderRecursively(s);
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
}
public interface Matchable<T>
{
    void setMatch(T match);

    float getScore(T matchees);
}
