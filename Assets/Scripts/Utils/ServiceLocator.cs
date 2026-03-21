using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Utils
{
    // 서비스 로케이터 패턴
    // Manager들을 인터페이스로 등록/조회해서 직접 참조 의존성을 제거
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} 이미 등록됨. 덮어씁니다.");
            }
            _services[type] = service;
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
                return (T)service;

            Debug.LogError($"[ServiceLocator] {type.Name} 등록되지 않음.");
            return default;
        }

        public static bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }

        public static void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }

        /// <summary>씬 전환 시 호출해서 씬 종속 서비스만 초기화합니다.</summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
