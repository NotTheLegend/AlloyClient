using System;

namespace AlloyClient.Utils;

public static class Extensions {
    extension(Random random) {
        public int NextRange(int max) {
            return random.Next(max + 1);
        }
        
        public int NextRange(int min, int max) {
            return random.Next(min, max + 1);
        }
    }
}