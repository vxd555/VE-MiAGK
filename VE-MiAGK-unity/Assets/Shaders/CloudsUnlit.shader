Shader "Unlit/CloudsUnlit"
{
    Properties
    {
		WeatherMap("Weather map", 2D) = "" {}
		LowFreqNoiseTexture("LowFreqNoiseTexture", 3D) = "" {}
		HighFreqNoiseTexture("HighFreqNoiseTexture", 3D) = "" {}

		time("Time", Range(0.0, 1000.0)) = 0.0
		density("Density", Range(0.0, 20.0)) = 1.97
		coverage("Coverage", Range(0.0, 1.0)) = 0.5
		phaseInfluence("Phase influence", Range(0.0, 1.0)) = 0.95
		phaseInfluence2("Phase influence 2", Range(0.0, 1.0)) = 0.9
		eccentrisy("Eccentrisy", Range(0.0, 1.0)) = 0.98
		eccentrisy2("Eccentrisy 2", Range(0.0, 1.0)) = 0.3
		attenuation("Attenuation", Range(0.0, 100.0)) = 50.0
		attenuation2("Attenuation 2", Range(0.0, 100.0)) = 15.8
		sunIntensity("Sun intensity", Range(0, 500)) = 50.0
		_fog("Fog", Range(0.0, 30.0)) = 7.97
		_ambient("Ambient", Range(0.0, 10.0)) = 1.98
		sunHeight("Sun height", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			// exclude Direct3D 11 9.x feature level and d3d9 to compile loops without unroll
			#pragma exclude_renderers d3d11_9x
			#pragma exclude_renderers d3d9

            #include "UnityCG.cginc"

			struct VS_INPUT
			{
				float4 Position : POSITION;
				float2 TextureCoordinate : TEXCOORD;
			};

			struct VS_OUTPUT
			{
				float4 Position : SV_Position;
				float2 TextureCoordinate : TEXCOORD0;
				float3 WorldPosition : TEXCOORD1;
			};

			sampler2D WeatherMap;
			sampler3D LowFreqNoiseTexture;
			sampler3D HighFreqNoiseTexture;

			float time;
			float density;
			float coverage;
			float phaseInfluence;
			float phaseInfluence2;
			float eccentrisy;
			float eccentrisy2;
			float attenuation;
			float attenuation2;
			int sunIntensity;
			float _fog;
			float _ambient;
			float sunHeight;

			static const float earthR = 6400;
			static const float cloudMin = earthR + 15;
			static const float cloudMax = earthR + 35;
			static const float PI = 3.14159265f;
			static const float primSteps = 16;
			static const float secSteps = 8;
			static const float3 kRlh = float3(5.5e-6, 13.0e-6, 22.4e-6); 	// Rayleigh scattering coefficient
			static const float kMie = 21e-6;                          		// Mie scattering coefficient
			static const float shRlh = 8e3;                            		// Rayleigh scale height
			static const float shMie = 1.2e3;                          		// Mie scale height
			static const float g = 0.99;                           			// Mie preferred scattering direction

			float2 rsi(float3 r0, float3 rd, float sr) {
				float a = dot(rd, rd);
				float b = 2.0 * dot(rd, r0);
				float c = dot(r0, r0) - (sr * sr);
				float d = (b*b) - 4.0*a*c;
				if (d < 0.0)
					return float2(1e5, -1e5);

				return float2(
					(-b - sqrt(d)) / (2.0*a),
					(-b + sqrt(d)) / (2.0*a)
					);
			}

			float RayleighPhaseFunction(float cosTheta)
			{
				return 3.0 / (16.0 * PI) * (1.0 + (cosTheta * cosTheta));
			}

			float MiePhaseFunction(float cosTheta, float g)
			{
				return 3.0 / (8.0 * PI) * ((1.0 - g * g) * (cosTheta * cosTheta + 1.0)) / (pow(1.0 + g * g - 2.0 * cosTheta * g, 1.5) * (2.0 + g * g));
			}

			float3 atmosphere(float3 r, float3 r0, float3 pSun, float iSun, float rPlanet, float rAtmos) {
				pSun = normalize(pSun);
				r = normalize(r);

				float2 p = rsi(r0, r, rAtmos);
				if (p.x > p.y)
					return float3(0, 0, 0);
				p.y = min(p.y, rsi(r0, r, rPlanet).x);
				float primStepSize = (p.y - p.x) / float(primSteps);

				float primTime = 0.0;

				float3 totalRlh = float3(0, 0, 0);
				float3 totalMie = float3(0, 0, 0);

				float primOdRlh = 0.0;
				float primOdMie = 0.0;

				float pRlh = RayleighPhaseFunction(dot(r, pSun));
				float pMie = MiePhaseFunction(dot(r, pSun), g);

				for (int i = 0; i < primSteps; i++) {

					float3 primPos = r0 + r * (primTime + primStepSize * 0.5);

					float primHeight = length(primPos) - rPlanet;

					float odStepRlh = exp(-primHeight / shRlh) * primStepSize;
					float odStepMie = exp(-primHeight / shMie) * primStepSize;

					primOdRlh += odStepRlh;
					primOdMie += odStepMie;

					float secStepSize = rsi(primPos, pSun, rAtmos).y / float(secSteps);

					float secTime = 0.0;

					float secOdRlh = 0.0;
					float secOdMie = 0.0;

					for (int j = 0; j < secSteps; j++) {

						float3 secPos = primPos + pSun * (secTime + secStepSize * 0.5);

						float secHeight = length(secPos) - rPlanet;

						secOdRlh += exp(-secHeight / shRlh) * secStepSize;
						secOdMie += exp(-secHeight / shMie) * secStepSize;

						secTime += secStepSize;
					}

					float3 attn = exp(-(kMie * (primOdMie + secOdMie) + kRlh * (primOdRlh + secOdRlh)));

					totalRlh += odStepRlh * attn;
					totalMie += odStepMie * attn;

					primTime += primStepSize;

				}

				return iSun * (pRlh * kRlh * totalRlh + pMie * kMie * totalMie);
			}

			float remap(float value, float minValue, float maxValue, float newMinValue, float newMaxValue)
			{
				return newMinValue + (value - minValue) / (maxValue - minValue) * (newMaxValue - newMinValue);
			}

			float getCloudHeightFraction(float3 position)
			{
				return clamp((length(position) - cloudMin) / (cloudMax - cloudMin), 0, 1);
			}

			float cloudPhaseFunction(float cosTheta, float g)
			{
				float denom = 1 + g * g - 2 * g * cosTheta;
				return (1 / (4 * PI)) * (1 - g * g) / (denom * sqrt(denom));
			}

			bool crossRaySphereOut(float3 rayStart, float3 rayDirection, float sphereRadius, out float3 result1, out float3 result2)
			{
				result1 = (float3)0.0;
				result2 = (float3)0.0;

				float3 sphereCenter = float3(0.0, 0.0, 0.0);

				float3 rayPoint = rayStart + dot(sphereCenter - rayStart, rayDirection) * rayDirection;
				float height = length(rayPoint - sphereCenter);
				if (height <= sphereRadius)
				{
					float dist = sqrt(sphereRadius * sphereRadius - height * height);
					result1 = rayPoint - rayDirection * dist;
					result2 = rayPoint + rayDirection * dist;
					return dot(result1 - rayStart, rayDirection) > 0.0f || dot(result2 - rayStart, rayDirection) > 0.0f;
				}
				return false;
			}

			bool crossRaySphereOutFar(float3 rayStart, float3 rayDirection, float sphereRadius, out float3 result)
			{
				float3 tmpPoint;
				return crossRaySphereOut(rayStart, rayDirection, sphereRadius, tmpPoint, result);
			}

			float sampleCloudDensity(float3 position, bool simple = false, float lod = 1.0)
			{
				float3 weatherPosition = position;
				position.xz += float2(0.2f, 0.2f) * (time + _Time.w);

				float4 weather = tex2Dlod(WeatherMap, float4(weatherPosition.xz / 4096.0f + float2(0.2, 0.1), 0, 0));
				float wCoverageLow = weather.r;
				float wCoverageHigh = weather.g;
				float wHeight = weather.b;
				float wDensity = weather.a;
				float height = getCloudHeightFraction(position);

				float wCoverage = max(wCoverageLow, clamp(coverage - 0.5, 0, 1) * wCoverageHigh * 2);

				float ShapeAlt = clamp(remap(height, 0, 0.07, 0, 1), 0, 1);
				ShapeAlt *= clamp(remap(height, wHeight * 0.2, wHeight, 1, 0), 0, 1);

				float DensityAlt = height * clamp(remap(height, 0, 0.15, 0, 1), 0, 1);
				DensityAlt *= height * clamp(remap(height, 0.9, 1, 1, 0), 0, 1);
				DensityAlt *= wDensity * 2 * density;

				float ShapeNoise = tex3Dlod(LowFreqNoiseTexture, float4(position / 84.0f, lod)).x;
				ShapeNoise = clamp(remap(ShapeNoise * ShapeAlt, 1 - coverage * wCoverage, 1, 0, 1), 0, 1) * DensityAlt;

				float density;
				if (!simple)
				{
					float DensityNoise = tex3Dlod(HighFreqNoiseTexture, float4(position / 10.8f, lod)).x;
					DensityNoise = 0.35 * exp(-coverage * 0.75) * lerp(DensityNoise, 1 - DensityNoise, clamp(height * 5, 0, 1));

					density = clamp(remap(ShapeNoise, DensityNoise, 1, 0, 1), 0, 1) * DensityAlt;

					return density;
				}

				density = ShapeNoise * DensityAlt;

				return density;
			}

			static const float3 NoiseKernel[] =
			{
				float3(0.38051305f,  0.92453449f, -0.02111345f),
				float3(-0.50625799f, -0.03590792f, -0.86163418f),
				float3(-0.32509218f, -0.94557439f,  0.01428793f),
				float3(0.09026238f, -0.27376545f,  0.95755165f),
				float3(0.28128598f,  0.42443639f, -0.86065785f),
				float3(-0.16852403f,  0.14748697f,  0.97460106f)
			};

			float sampleCloudDensityCone(float3 position, float3 sunDir)
			{
				float avrStep = (cloudMax - cloudMin) / 100;
				float sumDensity = 0.0;

				for (int i = 0; i < 6; i++)
				{
					float step = avrStep;

					if (i == 5)
						step = step * 6.0;

					position += sunDir * step + (avrStep * NoiseKernel[i] * float(i));

					if (sumDensity < 0.3)
					{
						sumDensity += sampleCloudDensity(position) * step;
					}
					else
					{
						sumDensity += sampleCloudDensity(position, true) * step;
					}
				}

				return sumDensity;
			}

			float sampleCloudDensityRay(float3 position, float3 sunDir)
			{
				float avrStep = (cloudMax - cloudMin) / 100;
				float sumDensity = 0.0;

				for (int i = 0; i < 6; i++)
				{
					float step = avrStep;

					if (i == 5)
						step = step * 6.0;

					position += sunDir * step;
					float density = sampleCloudDensity(position) * step;
					sumDensity += density;
				}

				return sumDensity;
			}

			float4 marching(float3 viewDir, float3 sunDir, float3 rayOrigin, float3 sunColor, float3 _ambientColor)
			{
				float3 position;
				crossRaySphereOutFar(rayOrigin, viewDir, cloudMin, position);

				float avrStep = (cloudMax - cloudMin) / 64.0; // / 64.0
				float steps = min(1 / avrStep * distance(rayOrigin, position), 128.0);

				float3 color = float3(0.0, 0.0, 0.0);
				float transmittance = 1.0;

				//for (int i = 0; i < steps; i++) // avrStep * 2
				for (int i = 0; i < 128; i++) // avrStep * 2
				{
					float density = sampleCloudDensity(position) * avrStep;
					if (density > 0.0)
					{
						float secRayDensity = sampleCloudDensityRay(position, sunDir);

						float cosTheta = max(0.0, dot(viewDir, sunDir));

						float phaseHG = phaseInfluence * cloudPhaseFunction(cosTheta, eccentrisy);
						phaseHG += phaseInfluence2 * cloudPhaseFunction(cosTheta, eccentrisy2);
						float secRayTransmittance = exp(-attenuation * secRayDensity);
						float sigma = attenuation2 * density;

						float light = sunIntensity * transmittance * sigma * phaseHG * secRayTransmittance;

						color += sunColor * light;

						transmittance *= exp(-attenuation * density);
					}

					position += viewDir * avrStep;

					if (transmittance < 0.05 || length(position) > cloudMax)
						break;
				}

				float blending = 1.0 - exp(-max(0.0, dot(viewDir, float3(0.0, 1.0, 0.0))) * _fog);
				blending = blending * blending * blending;
				return float4(lerp(_ambientColor, color + _ambientColor * _ambient, blending), 1.0 - transmittance);
			}

			float4 marchingOpt(float3 viewDir, float3 sunDir, float3 rayOrigin, float3 sunColor, float3 _ambientColor)
			{
				float3 position;
				crossRaySphereOutFar(rayOrigin, viewDir, cloudMin, position);

				float avrStep = (cloudMax - cloudMin) / 48.0; // / 64.0
				float steps = min(1 / avrStep * distance(rayOrigin, position), 64.0); // 192

				float3 color = float3(0.0, 0.0, 0.0);
				float transmittance = 1.0;

				int zeroSamplesCount = 0;
				float sampleTest = 0;

				for (int i = 0; i < steps; i++) // avrStep * 2
				{
					if (sampleTest > 0)
					{
						float density = sampleCloudDensity(position) * avrStep;

						if (density > 0)
						{
							float secRayDensity = sampleCloudDensityCone(position, sunDir);

							float cosTheta = max(0.0, dot(viewDir, sunDir));

							float phaseHG = phaseInfluence * cloudPhaseFunction(cosTheta, eccentrisy);
							phaseHG += phaseInfluence2 * cloudPhaseFunction(cosTheta, eccentrisy2);
							float secRayTransmittance = exp(-attenuation * secRayDensity);
							float sigma = attenuation2 * density;

							float light = sunIntensity * transmittance * sigma * phaseHG * secRayTransmittance;

							color += sunColor * light;

							transmittance *= exp(-attenuation * density);
						}
						else
						{
							zeroSamplesCount++;
						}

						if (zeroSamplesCount != 6)
						{
							position += viewDir * avrStep;
						}
						else
						{
							zeroSamplesCount = 0;
							sampleTest = 0;
						}
					}
					else
					{
						sampleTest = sampleCloudDensity(position, true);

						if (sampleTest <= 0)
							position += viewDir * avrStep;
					}

					if (transmittance < 0.05 || length(position) > cloudMax)
						break;
				}

				float blending = 1.0 - exp(-max(0.0, dot(viewDir, float3(0.0, 1.0, 0.0))) * _fog);
				blending = blending * blending * blending;
				return float4(lerp(_ambientColor, color + _ambientColor * _ambient, blending), 1.0 - transmittance);
			}

			VS_OUTPUT vert(VS_INPUT IN)
			{
				VS_OUTPUT OUT = (VS_OUTPUT)0;

				OUT.Position = UnityObjectToClipPos(IN.Position);
				OUT.WorldPosition = mul(IN.Position, unity_ObjectToWorld).xyz;
				OUT.TextureCoordinate = IN.TextureCoordinate;

				return OUT;
			}

			fixed4 frag(VS_OUTPUT IN) : SV_Target
			{
				float3 viewDirection = normalize(IN.WorldPosition - _WorldSpaceCameraPos);

				float3 sunDirection = normalize(float3(1.0 - sunHeight, sunHeight, 0.0f));

				float3 sunColor = float3(1.0, 0.7, 0.5);

				float3 skyColor = atmosphere(
					normalize(viewDirection),           	// normalized ray direction
					float3(0, 6372e3, 0),               	// ray origin
					sunDirection,                        	// position of the sun
					sunIntensity,                           // intensity of the sun
					6371e3,                         		// radius of the planet in meters
					6471e3	                         		// radius of the atmosphere in meters
				);

				//float4 color = float4(skyColor, 1.0);
				float4 color = marchingOpt(viewDirection, sunDirection, float3(0.0, 6400.0, 0.0), sunColor, float3(skyColor.r, skyColor.g * 1.2, skyColor.b * 1.3));

				//return pow(color / (1 + color), float(1.0 / 2.2));
				return color / (1 + color);
			}
            ENDCG
        }
    }
}
