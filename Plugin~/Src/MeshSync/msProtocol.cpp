#include "pch.h"
#include "MeshSync/msProtocol.h"

#include "MeshSync/SceneGraph/msScene.h" //Scene

namespace ms {

Message::~Message()
{
}
void Message::serialize(std::ostream& os) const
{
    write(os, protocol_version);
    write(os, session_id);
    write(os, message_id);
    write(os, timestamp_send);
}
void Message::deserialize(std::istream& is)
{
    read(is, protocol_version);
    if (protocol_version != msProtocolVersion) {
        throw std::runtime_error("Protocol version doesn't match");
    }
    read(is, session_id);
    read(is, message_id);
    read(is, timestamp_send);
}

GetMessage::GetMessage()
{
    SetAllGetFlags(flags);
}
void GetMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    write(os, flags);
    write(os, scene_settings);
    write(os, refine_settings);
}
void GetMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    read(is, flags);
    read(is, scene_settings);
    read(is, refine_settings);
}


SetMessage::SetMessage()
{
    scene = Scene::create();
}
SetMessage::SetMessage(ScenePtr s)
{
    scene = s;
}
void SetMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    msWrite(scene);
}
void SetMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    msRead(scene);
}


DeleteMessage::DeleteMessage()
{
}
void DeleteMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    write(os, entities);
    write(os, materials);
    write(os, instances);
}
void DeleteMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    read(is, entities);
    read(is, materials);
    read(is, instances);
}


FenceMessage::~FenceMessage() {}
void FenceMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    write(os, type);
}
void FenceMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    read(is, type);
}

TextMessage::~TextMessage() {}
void TextMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    write(os, text);
    write(os, type);
}
void TextMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    read(is, text);
    read(is, type);
}


ScreenshotMessage::ScreenshotMessage() {}
void ScreenshotMessage::serialize(std::ostream& os) const { super::serialize(os); }
void ScreenshotMessage::deserialize(std::istream& is) { super::deserialize(is); }


ResponseMessage::ResponseMessage()
{
}

void ResponseMessage::serialize(std::ostream & os) const
{
    super::serialize(os);
    write(os, text);
}

void ResponseMessage::deserialize(std::istream & is)
{
    super::deserialize(is);
    read(is, text);
}


QueryMessage::QueryMessage()
{
}

void QueryMessage::serialize(std::ostream & os) const
{
    super::serialize(os);
    write(os, query_type);
}

void QueryMessage::deserialize(std::istream & is)
{
    super::deserialize(is);
    read(is, query_type);
}


PollMessage::PollMessage()
{}

void PollMessage::serialize(std::ostream& os) const
{
    super::serialize(os);
    write(os, poll_type);
}

void PollMessage::deserialize(std::istream& is)
{
    super::deserialize(is);
    read(is, poll_type);
}


} // namespace ms
