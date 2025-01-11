/*
  Copyright 2024 DrMeepso

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.

  Source: https://github.com/DrMeepso/WebFishingCove/blob/main/Cove/GodotFormat/Common.cs
*/
namespace GLOKON.Baiters.GodotInterop
{
    internal enum GodotTypes
    {
        Null = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        String = 4,
        Vector2 = 5,
        Rect2 = 6,
        Vector3 = 7,
        Transform2D = 8,
        Plane = 9,
        Quaternion = 10,
        AA_BB = 11,
        Basis = 12,
        Transform = 13,
        Color = 14,
        NodePath = 15,
        RID = 16, // ns
        Object = 17, //ns
        Dictionary = 18,
        Array = 19
    }
}
