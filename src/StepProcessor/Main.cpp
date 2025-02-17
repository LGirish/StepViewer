#include <STEPControl_Reader.hxx>
#include <TopoDS_Shape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS.hxx>
#include <BRep_Tool.hxx>
#include <Poly_Triangulation.hxx>
#include <TColgp_Array1OfPnt.hxx>

#include <nlohmann/json.hpp>


namespace local
{
	const std::string b = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";//=

	static void serialize(const std::string& path, const nlohmann::json& jsonData)
	{
		std::vector<uint8_t> bytes;
		nlohmann::json::to_msgpack(jsonData, bytes);
		std::ofstream ofs(path, std::ios::binary);
		ofs.write(reinterpret_cast<const char*>(bytes.data()),
			static_cast<std::streamsize>(bytes.size()));
		ofs.close();
	}

	static int serializeToPipe(const std::string& namedPipe, const nlohmann::json& jsonData)
	{
		std::string pipepath = "\\\\.\\pipe\\" + namedPipe;

		HANDLE hPipe = CreateFileA(pipepath.c_str(), GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);

		if (hPipe == INVALID_HANDLE_VALUE)
			return -10;

		std::vector<uint8_t> bytes;
		nlohmann::json::to_msgpack(jsonData, bytes);

		DWORD bytesWritten;

		BOOL success = WriteFile(hPipe, bytes.data(), static_cast<DWORD>(bytes.size()), &bytesWritten, NULL);

		CloseHandle(hPipe);
		return success == TRUE ? 0 : -20;
	}

	std::string fromBase64(const std::string& in)
	{
		std::string decoded;
		std::vector<int> T(256, -1);
		for (int i = 0; i < 64; i++) T[b[i]] = i;

		int val = 0, valb = -8;
		for (const unsigned char c : in)
		{
			if (T[c] == -1) break;
			val = (val << 6) + T[c];
			valb += 6;
			if (valb >= 0) {
				decoded.push_back(static_cast<char>((val >> valb) & 0xFF));
				valb -= 8;
			}
		}
		return decoded;
	}
}

static std::pair<std::vector<std::array<double, 3>>, std::vector<std::array<int, 3>>> tessellate(const char* filename)
{
	std::vector<std::array<double, 3>> vertices;
	std::vector<std::array<int, 3>> faces;

	STEPControl_Reader reader;
	auto status = reader.ReadFile(filename);

	if (status != IFSelect_RetDone)
	{
		throw std::runtime_error("Failed to read STEP file");
	}

	reader.TransferRoots();
	auto shape = reader.OneShape();

	BRepMesh_IncrementalMesh(shape, 0.1);

	TopExp_Explorer faceExplorer(shape, TopAbs_FACE);
	for (; faceExplorer.More(); faceExplorer.Next())
	{
		auto face = TopoDS::Face(faceExplorer.Current());
		TopLoc_Location loc;
		Handle(Poly_Triangulation) triangulation = BRep_Tool::Triangulation(face, loc);

		if (triangulation.IsNull())
		{
			continue;
		}

		const auto& nodes = triangulation->Nodes();
		const auto& triangles = triangulation->Triangles();

		int vertexOffset = static_cast<int>(vertices.size());

		for (int i = nodes.Lower(); i <= nodes.Upper(); ++i)
		{
			gp_Pnt p = nodes(i).Transformed(loc);
			vertices.push_back({ p.X(), p.Y(), p.Z() });
		}

		for (int i = triangles.Lower(); i <= triangles.Upper(); ++i)
		{
			int n1, n2, n3;
			triangles(i).Get(n1, n2, n3);
			faces.push_back({ vertexOffset + n1 - 1, vertexOffset + n2 - 1, vertexOffset + n3 - 1 });
		}
	}

	return { vertices, faces };
}

int main(int argc, char* argv[])
{
	if (argc != 2 || argv[1] == nullptr)
		return -1;

	try
	{
		auto decodedStr = local::fromBase64(argv[1]);
		auto paramJson = nlohmann::json::parse(decodedStr);

		auto inputFile = std::string(paramJson["InputFile"]);
		auto namedPipe = std::string(paramJson["NamedPipe"]);

		auto [vertices, faces] = tessellate(inputFile.c_str());

		auto tessJson = nlohmann::json{};
		tessJson["Points"] = vertices;
		tessJson["Triangles"] = faces;

		return local::serializeToPipe(namedPipe, tessJson);

	}
	catch (const std::exception& e)
	{
		std::cerr << "Error: " << e.what() << std::endl;
		return -100;
	}

	return 0;
}
