using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZoneUA.Testing;

namespace ZoneUA.Tests.PlayMode
{
    public sealed class IsolatedScenePlayModeTests
    {
        private const string SceneRoot = "Assets/_ZoneUA/Scenes/Tests/";

        [UnityTest]
        public IEnumerator WorldGenerationScene_GeneratesDeterministicRuntimeWorld()
        {
            yield return LoadScene("WorldGenerationTestScene");
            Component generator = FindComponent("MapGenerator");
            Assert.That(generator, Is.Not.Null);
            yield return null;
            yield return null;
            int generatedChildren = generator.transform.childCount;
            Assert.That(generatedChildren, Is.GreaterThan(0));
            generator.SendMessage("Regenerate");
            yield return null;
            Assert.That(generator.transform.childCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator PlayerMovementScene_PlayerRespondsToMovementCommand()
        {
            yield return LoadScene("PlayerMovementTestScene");
            GameObject player = GameObject.Find("TestPlayer");
            Component controller = player.GetComponent("CharacterCustomController");
            Assert.That(controller, Is.Not.Null);
            Vector3 start = player.transform.position;
            yield return null;
            controller.SendMessage("SetMovementInput", Vector2.right);
            yield return new WaitForSeconds(0.25f);
            controller.SendMessage("ClearInput");
            Assert.That(player.transform.position.x, Is.GreaterThan(start.x));
        }

        [UnityTest]
        public IEnumerator CombatScene_PlayerAndNpcHaveDamageableRuntimeActors()
        {
            yield return LoadScene("CombatTestScene");
            GameObject player = GameObject.Find("TestPlayer");
            GameObject npc = GameObject.Find("TestNpcTarget");
            Assert.That(player.GetComponent("Health"), Is.Not.Null);
            Assert.That(npc.GetComponent("Health"), Is.Not.Null);
            Component weapon = FindComponentInChildrenByName(player, "WeaponController");
            Assert.That(weapon, Is.Not.Null);
            yield return null;
            Assert.That(weapon.GetType().GetMethod("Fire"), Is.Not.Null);
            Assert.That(FindComponentInChildrenByName(player, "ProjectileSpawner"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator NpcCombatScene_NpcsAcquireExplicitTargets()
        {
            yield return LoadScene("NpcCombatTestScene");
            GameObject alpha = GameObject.Find("NpcAlpha");
            GameObject bravo = GameObject.Find("NpcBravo");
            Component alphaController = alpha.GetComponent("NPCController");
            Component bravoController = bravo.GetComponent("NPCController");
            Assert.That(alphaController, Is.Not.Null);
            Assert.That(bravoController, Is.Not.Null);
            InvokeTargetSetter(alphaController, bravo.transform);
            InvokeTargetSetter(bravoController, alpha.transform);
            Assert.That(GetCurrentTarget(alphaController), Is.EqualTo(bravo.transform));
            Assert.That(GetCurrentTarget(bravoController), Is.EqualTo(alpha.transform));
            yield return null;
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(SceneRoot + sceneName + ".unity", LoadSceneMode.Single);
            yield return operation;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
            Assert.That(UnityEngine.Object.FindAnyObjectByType<ZoneUATestScenarioMarker>(), Is.Not.Null);
        }

        private static Component FindComponent(string typeName)
        {
            Component[] components = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().Name == typeName)
                    return components[i];
            }
            return null;
        }

        private static Component FindComponentInChildrenByName(GameObject root, string typeName)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            Component fallback = null;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().Name == typeName)
                {
                    fallback ??= components[i];
                    if (components[i].gameObject.activeInHierarchy && components[i] is Behaviour behaviour && behaviour.enabled)
                        return components[i];
                }
            }
            return fallback;
        }

        private static void InvokeTargetSetter(Component controller, Transform target)
        {
            Type targetType = controller.GetType().GetNestedType("TargetType", BindingFlags.Public);
            object npcValue = Enum.Parse(targetType, "NPC");
            controller.GetType().GetMethod("SetTarget").Invoke(controller, new[] { target, npcValue });
        }

        private static Transform GetCurrentTarget(Component controller)
        {
            return (Transform)controller.GetType().GetProperty("CurrentTarget").GetValue(controller);
        }
    }
}
