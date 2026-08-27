using FarmApp.Shared.Attributes;

namespace FarmApp.Shared.Enums;

public enum MapModalType
{
    [MapModalDimensions(Width = 120, Height = 90, ArrowRectDepth = 15, ArrowRectLength = 20)]
    Loading,
    [MapModalDimensions(Width = 180, Height = 160, ArrowRectDepth = 15, ArrowRectLength = 20)]
    Stead,
    [MapModalDimensions(Width = 180, Height = 160, ArrowRectDepth = 15, ArrowRectLength = 20)]
    Property,
    [MapModalDimensions(Width = 180, Height = 160, ArrowRectDepth = 15, ArrowRectLength = 20)]
    CustomStead,
    [MapModalDimensions(Width = 32, Height = 32, ArrowRectDepth = 15, ArrowRectLength = 20)]
    CloseButton,
    [MapModalDimensions(Width = 160, Height = 120, ArrowRectDepth = 15, ArrowRectLength = 20)]
    AdPrompt,
    [MapModalDimensions(Width = 160, Height = 70, ArrowRectDepth = 15, ArrowRectLength = 20)]
    AdPromptFail
}