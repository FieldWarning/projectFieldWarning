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

using PFW.Model.Armory;

namespace PFW.Units
{
    public class VoiceComponent : MonoBehaviour
    {
        private List<AudioClip>
            _selectAudio = null,
            _moveAudio = null,
            _attackAudio = null;
        // Randomly choose clips from list
        [SerializeField]
        private AudioSource _audioSource = null;

        public void Initialize(VoiceLines voiceLines)
        {
            _selectAudio = voiceLines.selectionLines;
            _moveAudio = voiceLines.movementLines;
            _attackAudio = voiceLines.aggressiveLines;
        }

        public void PlaySelectionVoiceline()
        {
            if (_selectAudio.Count != 0) 
            {
                int r = Random.Range(0, _selectAudio.Count);
                _audioSource.clip = _selectAudio[r];
                _audioSource.Play();
            }
        }

        public void PlayMoveCommandVoiceline()
        {
            if (_moveAudio.Count != 0) 
            {
                int r = Random.Range(0, _moveAudio.Count);
                _audioSource.clip = _moveAudio[r];
                _audioSource.Play();
            }
        }

        public void PlayAttackCommandVoiceline()
        {
            if (_attackAudio.Count != 0) 
            {
                int r = Random.Range(0, _attackAudio.Count);
                _audioSource.clip = _attackAudio[r];
                _audioSource.Play();
            }
        }
    }
}
