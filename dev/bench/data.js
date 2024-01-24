window.BENCHMARK_DATA = {
  "lastUpdate": 1706084686398,
  "repoUrl": "https://github.com/destructurama/attributed",
  "entries": {
    "Benchmarks": [
      {
        "commit": {
          "author": {
            "email": "sungam3r@yandex.ru",
            "name": "Ivan Maximov",
            "username": "sungam3r"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "1acf021abd6c1a990ebf5c6e0702caecda0680b5",
          "message": "Add benchmarks (#94)",
          "timestamp": "2024-01-24T11:21:36+03:00",
          "tree_id": "ad88900ed50828cf115571bc1c207bfcfde9856e",
          "url": "https://github.com/destructurama/attributed/commit/1acf021abd6c1a990ebf5c6e0702caecda0680b5"
        },
        "date": 1706084685741,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Benchmarks.AttributedBenchmarks.LogAsScalar",
            "value": 294.23576013247174,
            "unit": "ns",
            "range": "± 0.41404221815530473"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.LogMasked",
            "value": 489.53537823603705,
            "unit": "ns",
            "range": "± 1.0430909971636597"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.LogReplaced",
            "value": 615.3869253794352,
            "unit": "ns",
            "range": "± 1.4703301150692143"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.LogWithName",
            "value": 294.7703768185207,
            "unit": "ns",
            "range": "± 1.107501846531213"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.NotLogged",
            "value": 155.67615048701947,
            "unit": "ns",
            "range": "± 0.4962793307240302"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.NotLoggedIfDefault",
            "value": 169.36129275957742,
            "unit": "ns",
            "range": "± 0.7337499759199249"
          },
          {
            "name": "Benchmarks.AttributedBenchmarks.NotLoggedIfNull",
            "value": 237.39564723627907,
            "unit": "ns",
            "range": "± 0.4091085252073188"
          }
        ]
      }
    ]
  }
}