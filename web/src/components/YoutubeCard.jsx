import { ExternalLink } from 'lucide-react'

import SentimentBadge from './SentimentBadge'

function YoutubeCard({ video }) {
  return (
    <article className="
      overflow-hidden
      rounded-lg
      border
      border-zinc-200
      bg-zinc-50
      dark:border-zinc-800
      dark:bg-zinc-950
    ">
      <img
        src={video.thumbnail}
        alt={video.title}
        className="h-44 w-full object-cover"
      />

      <div className="p-5">
        <div className="
          mb-4
          flex
          items-start
          justify-between
          gap-4
        ">
          <h3 className="
            text-lg
            font-bold
            leading-snug
            text-zinc-950
            dark:text-white
          ">
            {video.title}
          </h3>

          <SentimentBadge
            sentiment={video.sentiment}
          />
        </div>

        <p className="
          mb-4
          text-sm
          text-zinc-500
          dark:text-zinc-400
        ">
          {video.channel}
        </p>

        <a
          href={video.url}
          target="_blank"
          rel="noreferrer"
          className="
            inline-flex
            items-center
            gap-2
            rounded-lg
            bg-red-600
            px-3
            py-2
            text-sm
            font-bold
            text-white
            hover:bg-red-700
          "
        >
          Assistir vídeo
          <ExternalLink size={15} />
        </a>
      </div>
    </article>
  )
}

export default YoutubeCard
