# StepViewer
CAx as a service

C++ Step Processor as an executable
1. Extract a minimum subset of 13 OpenCascade dll to import and Tessellate only StepModels
2. Accept user options as program arguments in Base64 encoded json
3. Write 3D Vertices and Triangle Indices as MessagePack binary into a Pipe using nlohmann Json for Modern C++

C# API Service StepAPIService
1. Use Microsoft.Extensions.Hosting to wrap IHost StartAsync() and StopAsync() API
2. Use Singleton hosted Service to host an IStepTessellator service
3. Hide the Service Implementation using Microsoft.Extensions.DependencyInjection
4. Provide asynchronous IStepTessellator::TessellateModel API to provide OpenCascade Tessellation feature as a multiprocess
5. Each API call can Tessellate a separate Step Model as a separate process.
6. Achieve IPC Interprocess communication via Named Pipes

Helix Step Viewer
1. Simple Helix Viewer which consumes the  OpenCascade microservice to tessellate a step model
and Render it as a Helix MeshGeometry3D