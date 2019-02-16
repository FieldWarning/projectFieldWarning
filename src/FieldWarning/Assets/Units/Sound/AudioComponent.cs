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

public class AudioComponent : MonoBehaviour
{
    [SerializeField] private AudioClip _selectAudio, _moveAudio;
    //private AudioSource _unitAudio;
    public void UnitSelectAudio(bool selected)
    {
        AudioSource _unitAudio = GetComponent<AudioSource>();
        _unitAudio.clip = _selectAudio;
        if (selected) {
            _unitAudio.Play();
            new WaitForSeconds(_unitAudio.clip.length); //it doesn't wait, need to loop
        }
    }

    public void UnitMoveAudio()
    {
        AudioSource _unitAudio = GetComponent<AudioSource>();
        _unitAudio.clip = _moveAudio;
        _unitAudio.Play();
        new WaitForSeconds(_unitAudio.clip.length); //it doesn't wait, need to loop
    }
}
