// Bridges the abstract Object-typed members of GoogleGson.TypeAdapter
// (Read(JsonReader) → Object, Write(JsonWriter, Object)) to the strongly-typed
// generated members on each adapter (ReadBase + Write(... typedValue)).
//
// Without these overrides the generated classes inherit unimplemented abstracts
// and fail with CS0534. Required on .NET 10 Android binding output where
// TypeAdapter is generated as the non-generic base.

namespace Mapbox.Geojson
{
    public partial class PointAsCoordinatesTypeAdapter
    {
        public override unsafe Java.Lang.Object? Read(global::GoogleGson.Stream.JsonReader? @in)
        {
            return ReadBase(@in);
        }

        public override unsafe void Write(global::GoogleGson.Stream.JsonWriter? @out, Java.Lang.Object? value)
        {
            Write(@out, value as Mapbox.Geojson.Point);
        }
    }
}

namespace Mapbox.Geojson.Gson
{
    public partial class BoundingBoxTypeAdapter
    {
        public override unsafe Java.Lang.Object? Read(global::GoogleGson.Stream.JsonReader? @in)
        {
            return ReadBase(@in);
        }

        public override unsafe void Write(global::GoogleGson.Stream.JsonWriter? @out, Java.Lang.Object? value)
        {
            Write(@out, value as Mapbox.Geojson.BoundingBox);
        }
    }
}
