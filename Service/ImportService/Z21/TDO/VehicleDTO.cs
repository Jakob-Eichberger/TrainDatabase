using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.ImportService.Z21.TDO
{
    [Table("vehicles")]
    internal class VehicleDTO
    {
        [Column("id")]
        internal long Id { get; set; }

        [Column("name")]
        internal string Name { get; set; }

        [Column("image_name")]
        internal string ImageName { get; set; }

        [Column("type")]
        internal long Type { get; set; }

        [Column("max_speed")]
        internal long MaxSpeed { get; set; }

        [Column("address")]
        internal long Address { get; set; }

        [Column("active")]
        internal long Active { get; set; }

        [Column("position")]
        internal long Position { get; set; }

        [Column("drivers_cab")]
        internal string DriversCab { get; set; }

        [Column("full_name")]
        internal string FullName { get; set; }

        [Column("speed_display")]
        internal long SpeedDisplay { get; set; }

        [Column("railway")]
        internal string Railway { get; set; }

        [Column("buffer_lenght")]
        internal string BufferLenght { get; set; }

        [Column("model_buffer_lenght")]
        internal string ModelBufferLenght { get; set; }

        [Column("service_weight")]
        internal string ServiceWeight { get; set; }

        [Column("model_weight")]
        internal string ModelWeight { get; set; }

        [Column("rmin")]
        internal string Rmin { get; set; }

        [Column("article_number")]
        internal string ArticleNumber { get; set; }

        [Column("decoder_type")]
        internal string DecoderType { get; set; }

        [Column("owner")]
        internal string Owner { get; set; }

        [Column("build_year")]
        internal string BuildYear { get; set; }

        [Column("owning_since")]
        internal string OwningSince { get; set; }

        [Column("traction_direction")]
        internal long TractionDirection { get; set; }

        [Column("description")]
        internal string Description { get; set; }

        [Column("dummy")]
        internal long Dummy { get; set; }

        [Column("ip")]
        internal string Ip { get; set; }

        [Column("video")]
        internal long Video { get; set; }

        [Column("video_x")]
        internal long VideoX { get; set; }

        [Column("video_y")]
        internal long VideoY { get; set; }

        [Column("video_width")]
        internal long VideoWidth { get; set; }

        [Column("panorama_x")]
        internal long PanoramaX { get; set; }

        [Column("panorama_y")]
        internal long PanoramaY { get; set; }

        [Column("panorama_width")]
        internal long PanoramaWidth { get; set; }

        [Column("panoramaImage")]
        internal string PanoramaImage { get; set; }

        [Column("direct_steering")]
        internal long DirectSteering { get; set; }

        [Column("crane")]
        internal long Crane { get; set; }
    }
}
